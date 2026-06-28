//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { FinancialExportFormat, FinancialExportSource } from '../../types/exporter';
import { AuthService } from '../auth/auth-service';

import { FinancialExportService } from './financial-export-service';

describe('FinancialExportService', () => {
  let service: FinancialExportService;
  let originalFetch: typeof globalThis.fetch;
  let originalCreateObjectUrl: typeof URL.createObjectURL;
  let originalRevokeObjectUrl: typeof URL.revokeObjectURL;
  let clickMock: ReturnType<typeof vi.fn>;

  const authMock = {
    getToken: vi.fn().mockReturnValue('test-token'),
  };

  beforeEach(() => {
    originalFetch = globalThis.fetch;
    originalCreateObjectUrl = URL.createObjectURL;
    originalRevokeObjectUrl = URL.revokeObjectURL;
    clickMock = vi.fn();

    vi.spyOn(document, 'createElement').mockReturnValue({
      click: clickMock,
      href: '',
      download: '',
    } as unknown as HTMLAnchorElement);
    URL.createObjectURL = vi.fn().mockReturnValue('blob:test-url');
    URL.revokeObjectURL = vi.fn();

    TestBed.configureTestingModule({
      providers: [FinancialExportService, { provide: AuthService, useValue: authMock }],
    });

    service = TestBed.inject(FinancialExportService);
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
    URL.createObjectURL = originalCreateObjectUrl;
    URL.revokeObjectURL = originalRevokeObjectUrl;
    vi.restoreAllMocks();
  });

  it('should download CSV with query parameters and filename from header', async () => {
    const blob = new Blob(['csv'], { type: 'text/csv' });
    globalThis.fetch = vi.fn().mockResolvedValue(
      new Response(blob, {
        status: 200,
        headers: {
          'Content-Disposition': 'attachment; filename="export.csv"',
        },
      }),
    );

    await firstValueFrom(
      service.downloadFinancialData(
        {
          TeamId: 7,
          CostCentreId: 4,
          Source: FinancialExportSource.SubmittedInvoices,
          MinPaidAt: '2026-01-01',
          MaxPaidAt: undefined,
          PurposeOfPayment: '',
        },
        FinancialExportFormat.Csv,
      ),
    );

    const expectedBaseUrl = environment.apiBaseUrl
      ? new URL('/api/v1/transaction/export', environment.apiBaseUrl).toString()
      : '/api/v1/transaction/export';
    const [url, init] = vi.mocked(globalThis.fetch).mock.calls[0];

    expect(String(url)).toContain(expectedBaseUrl);
    expect(String(url)).toContain('Format=1');
    expect(String(url)).toContain('TeamId=7');
    expect(String(url)).toContain('CostCentreId=4');
    expect(String(url)).toContain('Source=1');
    expect(String(url)).toContain('MinPaidAt=2026-01-01');
    expect(String(url)).not.toContain('MaxPaidAt');
    expect(String(url)).not.toContain('PurposeOfPayment');
    expect(init).toEqual(
      expect.objectContaining({
        method: 'GET',
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      }),
    );
    expect(clickMock).toHaveBeenCalledOnce();
    expect(URL.createObjectURL).toHaveBeenCalledWith(blob);
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test-url');
  });

  it('should use PDF fallback filename when content disposition is missing', async () => {
    globalThis.fetch = vi
      .fn()
      .mockResolvedValue(
        new Response(new Blob(['pdf'], { type: 'application/pdf' }), { status: 200 }),
      );

    await firstValueFrom(
      service.downloadFinancialData(
        { Source: FinancialExportSource.PaymentRequests },
        FinancialExportFormat.Pdf,
      ),
    );

    const anchor = vi.mocked(document.createElement).mock.results[0].value as HTMLAnchorElement;

    expect(anchor.download).toBe('payment-requests-export.pdf');
  });

  it('should use CSV fallback filename when content disposition is missing', async () => {
    globalThis.fetch = vi
      .fn()
      .mockResolvedValue(new Response(new Blob(['csv'], { type: 'text/csv' }), { status: 200 }));

    await firstValueFrom(
      service.downloadFinancialData(
        { Source: FinancialExportSource.SubmittedInvoices },
        FinancialExportFormat.Csv,
      ),
    );

    const anchor = vi.mocked(document.createElement).mock.results[0].value as HTMLAnchorElement;

    expect(anchor.download).toBe('submitted-invoices-export.csv');
  });

  it('should throw API detail when export fails', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ detail: 'Export failed' }), {
        status: 400,
        headers: { 'Content-Type': 'application/json' },
      }),
    );

    await expect(
      firstValueFrom(
        service.downloadFinancialData(
          { Source: FinancialExportSource.SubmittedInvoices },
          FinancialExportFormat.Csv,
        ),
      ),
    ).rejects.toThrow('Export failed');
  });

  it('should throw fallback error when export failure body is not JSON', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue(new Response('broken', { status: 500 }));

    await expect(
      firstValueFrom(
        service.downloadFinancialData(
          { Source: FinancialExportSource.SubmittedInvoices },
          FinancialExportFormat.Csv,
        ),
      ),
    ).rejects.toThrow('Financial export failed.');
  });
});
