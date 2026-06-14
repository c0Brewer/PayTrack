import { Injectable, signal } from '@angular/core';

import { CreatePaymentRequestByUserDto } from '../../types/exporter';

type OfflineInvoiceSubmissionStatus = 'pending';

interface OfflineInvoiceSubmissionRecord {
  id: string;
  createdAt: string;
  payload: CreatePaymentRequestByUserDto;
  file: Blob;
  fileName: string;
  fileType: string;
  status: OfflineInvoiceSubmissionStatus;
  lastError: string | null;
  lastAttemptAt: string | null;
}

export interface OfflineInvoiceSubmissionItem {
  id: string;
  createdAt: string;
  invoiceNumber: string;
  amount: number;
  teamId: number;
  fileName: string;
  status: OfflineInvoiceSubmissionStatus;
  lastError: string | null;
  lastAttemptAt: string | null;
}

export interface OfflineInvoiceSubmissionDraft {
  id: string;
  payload: CreatePaymentRequestByUserDto;
  file: File;
}

const DATABASE_NAME = 'paytrack-offline-submissions';
const DATABASE_VERSION = 1;
const STORE_NAME = 'user-invoice-submissions';

@Injectable({
  providedIn: 'root',
})
export class OfflineInvoiceSubmissionQueueService {
  readonly items = signal<OfflineInvoiceSubmissionItem[]>([]);

  private initialized = false;

  init(): void {
    if (this.initialized || typeof indexedDB === 'undefined' || typeof window === 'undefined') {
      return;
    }

    this.initialized = true;
    void this.refreshItems();
  }

  async queueSubmission(payload: CreatePaymentRequestByUserDto, file: File): Promise<void> {
    const record: OfflineInvoiceSubmissionRecord = {
      id: globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`,
      createdAt: new Date().toISOString(),
      payload,
      file,
      fileName: file.name,
      fileType: file.type,
      status: 'pending',
      lastError: null,
      lastAttemptAt: null,
    };

    const db = await this.openDatabase();
    await this.runRequest(
      db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME).put(record),
    );
    await this.refreshItems();
  }

  async getSubmissionDraft(id: string): Promise<OfflineInvoiceSubmissionDraft | null> {
    const record = await this.getRecord(id);
    if (!record) {
      return null;
    }

    return {
      id: record.id,
      payload: record.payload,
      file: new File([record.file], record.fileName, { type: record.fileType }),
    };
  }

  async removeSubmission(id: string): Promise<void> {
    const db = await this.openDatabase();
    await this.runRequest(
      db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME).delete(id),
    );
    await this.refreshItems();
  }

  private async refreshItems(): Promise<void> {
    const records = await this.getRecords();
    const items = records
      .map((record) => ({
        id: record.id,
        createdAt: record.createdAt,
        invoiceNumber: record.payload.invoiceNumber,
        amount: record.payload.transaction.amount,
        teamId: record.payload.transaction.teamId,
        fileName: record.fileName,
        status: record.status,
        lastError: record.lastError,
        lastAttemptAt: record.lastAttemptAt,
      }))
      .sort((a, b) => b.createdAt.localeCompare(a.createdAt));

    this.items.set(items);
  }

  private async getRecords(): Promise<OfflineInvoiceSubmissionRecord[]> {
    const db = await this.openDatabase();
    const transaction = db.transaction(STORE_NAME, 'readonly');
    const records = await this.runRequest<OfflineInvoiceSubmissionRecord[]>(
      transaction.objectStore(STORE_NAME).getAll(),
    );
    return records;
  }

  private async getRecord(id: string): Promise<OfflineInvoiceSubmissionRecord | undefined> {
    const db = await this.openDatabase();
    return await this.runRequest<OfflineInvoiceSubmissionRecord | undefined>(
      db.transaction(STORE_NAME, 'readonly').objectStore(STORE_NAME).get(id),
    );
  }

  private async openDatabase(): Promise<IDBDatabase> {
    return await new Promise((resolve, reject) => {
      const request = indexedDB.open(DATABASE_NAME, DATABASE_VERSION);

      request.onupgradeneeded = (): void => {
        const db = request.result;
        if (!db.objectStoreNames.contains(STORE_NAME)) {
          db.createObjectStore(STORE_NAME, { keyPath: 'id' });
        }
      };

      request.onsuccess = (): void => resolve(request.result);
      request.onerror = (): void =>
        reject(request.error ?? new Error('Failed to open offline queue.'));
    });
  }

  private async runRequest<T>(request: IDBRequest<T>): Promise<T> {
    return await new Promise((resolve, reject) => {
      request.onsuccess = (): void => resolve(request.result);
      request.onerror = (): void => reject(request.error ?? new Error('IndexedDB request failed.'));
    });
  }
}
