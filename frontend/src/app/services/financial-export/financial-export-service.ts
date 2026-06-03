import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth-service';

export type FinancialExportFormat = 'Csv' | 'Pdf';
export type FinancialExportQuery = Record<string, string | number | boolean | undefined | null>;

@Injectable({
  providedIn: 'root',
})
export class FinancialExportService {
  constructor(private readonly authService: AuthService) {}

  public downloadFinancialData(
    queryOptions: FinancialExportQuery,
    format: FinancialExportFormat,
  ): Observable<void> {
    const promise = fetch(this.getExportUrl(queryOptions, format), {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${this.authService.getToken()}`,
      },
    }).then(async (response) => {
      if (!response.ok) {
        const error = await response.json().catch(() => null);
        throw new Error(error?.detail ?? 'Financial export failed.');
      }

      const blob = await response.blob();
      this.downloadBlob(blob, this.getFileName(response, format));
    });

    return from(promise);
  }

  private getExportUrl(
    queryOptions: FinancialExportQuery,
    format: FinancialExportFormat,
  ): string {
    const path = '/api/v1/transaction/export';
    const baseUrl = environment.apiBaseUrl ? new URL(path, environment.apiBaseUrl).toString() : path;
    const params = new URLSearchParams();

    params.set('Format', format);

    Object.entries(queryOptions).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.set(key, String(value));
      }
    });

    return `${baseUrl}?${params.toString()}`;
  }

  private getFileName(response: Response, format: FinancialExportFormat): string {
    const contentDisposition = response.headers.get('Content-Disposition');
    const fileNameMatch = contentDisposition?.match(/filename="?([^"]+)"?/i);

    return fileNameMatch?.[1] ?? `financial-export.${format.toLowerCase()}`;
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = fileName;
    link.click();

    URL.revokeObjectURL(url);
  }
}
