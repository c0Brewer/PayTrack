import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import {
  BankStatementEntryDto,
  BankStatementMatchResponseDto,
  BankStatementUpdateRequestDto,
  TransactionDto,
} from '../../types/exporter';
import { BankStatementService } from './bank-statement-service';

const mockEntry: BankStatementEntryDto = {
  booking: '2026-05-21T00:00:00.000+02:00',
  partnerName: 'ACME Corp',
  partnerAccount: { iban: 'AT611904300234573201', bic: 'BKAUATWW' },
  amount: { value: 120.5, currency: 'EUR' },
  receiverReference: 'INV-2026-042',
  reference: null,
};

const mockTransaction: TransactionDto = {
  id: 7,
  userId: 1,
  teamId: 2,
  amount: 120.5,
  purposeOfPayment: 'ACME Invoice',
  paymentReference: 'INV-2026-042',
  status: 1,
  budgetId: null,
  paidAt: '2026-05-21T00:00:00.000Z',
};

const mockMatchResponse: BankStatementMatchResponseDto = {
  results: [
    {
      entry: mockEntry,
      hasMatch: true,
      matchedTransaction: mockTransaction,
      matchScore: 85,
    },
  ],
};

describe('BankStatementService', () => {
  let service: BankStatementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BankStatementService);
  });

  afterEach(() => vi.clearAllMocks());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ── getMatches ─────────────────────────────────────────────────────────────

  describe('getMatches', () => {
    it('should POST entries and return match response', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({ data: mockMatchResponse, error: null });

      const result = await firstValueFrom(service.getMatches([mockEntry]));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).POST).toHaveBeenCalledWith(
        '/api/v1/transaction/bank-statement-matches',
        { body: [mockEntry] },
      );
      expect(result).toEqual(mockMatchResponse);
    });

    it('should throw error with detail message from API', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({
        data: null,
        error: { detail: 'Invalid bank statement format' },
      });

      await expect(firstValueFrom(service.getMatches([mockEntry]))).rejects.toThrow(
        'Invalid bank statement format',
      );
    });

    it('should throw default message when error has no detail', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({ data: null, error: {} });

      await expect(firstValueFrom(service.getMatches([mockEntry]))).rejects.toThrow(
        'Failed to load bank statement matches',
      );
    });

    it('should throw when data is null without error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({ data: null, error: null });

      await expect(firstValueFrom(service.getMatches([mockEntry]))).rejects.toThrow(
        'Empty response from bank statement matches',
      );
    });
  });

  // ── applyUpdates ───────────────────────────────────────────────────────────

  describe('applyUpdates', () => {
    const updates: BankStatementUpdateRequestDto[] = [
      { entryId: 'entry-0', matchedTransactionId: 7, skipped: false },
      { entryId: 'entry-1', matchedTransactionId: null, skipped: true },
    ];

    it('should PUT updates and return updated transactions', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({ data: [mockTransaction], error: null });

      const result = await firstValueFrom(service.applyUpdates(updates));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).PUT).toHaveBeenCalledWith(
        '/api/v1/transaction/bank-statement-matches',
        { body: updates },
      );
      expect(result).toEqual([mockTransaction]);
    });

    it('should throw error with detail message from API', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({
        data: null,
        error: { detail: 'Transaction not found' },
      });

      await expect(firstValueFrom(service.applyUpdates(updates))).rejects.toThrow(
        'Transaction not found',
      );
    });

    it('should throw default message when error has no detail', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({ data: null, error: {} });

      await expect(firstValueFrom(service.applyUpdates(updates))).rejects.toThrow(
        'Failed to apply bank statement updates',
      );
    });

    it('should throw when data is null without error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({ data: null, error: null });

      await expect(firstValueFrom(service.applyUpdates(updates))).rejects.toThrow(
        'Empty response from bank statement update',
      );
    });
  });
});
