//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { client } from '../../client';
import { CreatePaymentRequestByUserDto, PaymentRequestByUserDto } from '../../types/exporter';
import { AuthService } from '../auth/auth-service';

import { PaymentRequestByUserService } from './payment-request-by-user-service';

describe('PaymentRequestByUserService', () => {
  let service: PaymentRequestByUserService;

  const authMock = {
    getToken: vi.fn().mockReturnValue('test-token'),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PaymentRequestByUserService, { provide: AuthService, useValue: authMock }],
    });

    service = TestBed.inject(PaymentRequestByUserService);
  });

  // -----------------------
  // CREATE
  // -----------------------
  it('should create payment request via fetch', async () => {
    const dto = {
      invoiceNumber: 'INV-1',
      comment: 'test',
      payoutType: 1,
      bankAccountId: 10,
      transaction: {
        teamId: 1,
        amount: 100,
        purposeOfPayment: 'test',
        paidAt: '2025-01-01',
      },
    } as CreatePaymentRequestByUserDto;

    const file = new File(['hello'], 'test.pdf', { type: 'application/pdf' });

    const apiResponse: PaymentRequestByUserDto = {
      id: 1,
      amount: 100,
      invoiceNumber: 'INV-1',
    } as PaymentRequestByUserDto;

    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => apiResponse,
    });

    const result = await firstValueFrom(service.createPaymentRequestByUser(dto, file));

    const expectedUrl = environment.apiBaseUrl
      ? new URL('/api/v1/transaction/user', environment.apiBaseUrl).toString()
      : '/api/v1/transaction/user';

    expect(globalThis.fetch).toHaveBeenCalledWith(
      expectedUrl,
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      }),
    );
    expect(result).toEqual(apiResponse);
  });

  it('should throw error on create if fetch fails', async () => {
    const dto = {
      invoiceNumber: 'INV-1',
      comment: '',
      payoutType: 1,
      bankAccountId: 10,
      transaction: {
        teamId: 1,
        amount: 100,
        purposeOfPayment: 'test',
        paidAt: '2025-01-01',
      },
    } as CreatePaymentRequestByUserDto;

    const file = new File(['hello'], 'test.pdf');

    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      json: async () => ({ detail: 'Upload failed' }),
    });

    await expect(firstValueFrom(service.createPaymentRequestByUser(dto, file))).rejects.toThrow(
      'Upload failed',
    );
  });

  // -----------------------
  // GET LIST
  // -----------------------
  it('should fetch payment requests list', async () => {
    const apiResponse = {
      items: [],
      totalCount: 0,
    };

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const result = await firstValueFrom(service.getPaymentRequestsByUser({} as any));

    expect(client.GET).toHaveBeenCalledWith('/api/v1/transaction/user', {
      params: { query: {} },
    });

    expect(result).toEqual(apiResponse);
  });

  it('should throw error on list fetch failure', async () => {
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: null,
      error: { detail: 'error' },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    await expect(firstValueFrom(service.getPaymentRequestsByUser({} as any))).rejects.toThrow(
      'error',
    );
  });

  // -----------------------
  // GET BY ID
  // -----------------------
  it('should fetch payment request by id', async () => {
    const apiResponse: PaymentRequestByUserDto = {
      id: 1,
      amount: 100,
    } as PaymentRequestByUserDto;

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const result = await firstValueFrom(service.getPaymentRequestsByUserById(1, {} as any));

    expect(client.GET).toHaveBeenCalledWith('/api/v1/transaction/user/{id}', {
      params: {
        path: { id: 1 },
        query: {},
      },
    });

    expect(result).toEqual(apiResponse);
  });

  it('should throw error on getById failure', async () => {
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: null,
      error: { detail: 'not found' },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      firstValueFrom(service.getPaymentRequestsByUserById(1, {} as any)),
    ).rejects.toThrow('not found');
  });

  // -----------------------
  // DUPLICATE CHECK
  // -----------------------
  it('should fetch duplicate payment requests', async () => {
    const apiResponse = [
      {
        paymentRequestByUser: {
          id: 1,
          amount: 100,
          invoiceNumber: 'INV-1',
          user: { id: 10, name: 'Alex' },
          team: { id: 20, name: 'Core Team' },
        },
        score: 160,
        isAmountAndUserMatch: true,
        isInvoiceNumberMatch: true,
        isAmountAndTeamMatch: true,
      },
    ];

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const query = { TeamId: 1, Amount: 100, PaidAt: '2025-01-01T00:00:00.000Z' } as any;
    const result = await firstValueFrom(service.getDuplicatePaymentRequestsByUser(query));

    expect(client.GET).toHaveBeenCalledWith('/api/v1/transaction/user/duplicate', {
      params: { query },
    });
    expect(result).toEqual(apiResponse);
  });

  it('should throw error on duplicate check failure', async () => {
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: null,
      error: { detail: 'duplicate check failed' },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(
      firstValueFrom(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        service.getDuplicatePaymentRequestsByUser({} as any),
      ),
    ).rejects.toThrow('duplicate check failed');
  });

  it('should throw error when duplicate check response has no data', async () => {
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: null,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(
      firstValueFrom(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        service.getDuplicatePaymentRequestsByUser({} as any),
      ),
    ).rejects.toThrow('Unexpected Error');
  });

  it('should delete payment request via fetch', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
    });

    await firstValueFrom(service.deletePaymentRequestByUser(5));

    const expectedUrl = environment.apiBaseUrl
      ? new URL('/api/v1/transaction/user/5', environment.apiBaseUrl).toString()
      : '/api/v1/transaction/user/5';

    expect(globalThis.fetch).toHaveBeenCalledWith(
      expectedUrl,
      expect.objectContaining({
        method: 'DELETE',
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      }),
    );
  });

  it('should dismiss duplicate warning via fetch', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
    });

    await firstValueFrom(service.dismissDuplicatePaymentRequestByUser(1, 2));

    const expectedUrl = environment.apiBaseUrl
      ? new URL('/api/v1/transaction/user/1/duplicate/2/dismiss', environment.apiBaseUrl).toString()
      : '/api/v1/transaction/user/1/duplicate/2/dismiss';

    expect(globalThis.fetch).toHaveBeenCalledWith(
      expectedUrl,
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          Authorization: 'Bearer test-token',
        }),
      }),
    );
  });

  // -----------------------
  // UPDATE
  // -----------------------
  it('should update payment request', async () => {
    const apiResponse: PaymentRequestByUserDto = {
      id: 1,
      amount: 999,
    } as PaymentRequestByUserDto;

    vi.spyOn(client, 'PUT').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const result = await firstValueFrom(service.updatePaymentRequestByUser(1, {} as any));

    expect(client.PUT).toHaveBeenCalledWith('/api/v1/transaction/user/{id}', {
      params: { path: { id: 1 } },
      body: {},
    });

    expect(result).toEqual(apiResponse);
  });

  it('should throw error on update failure', async () => {
    vi.spyOn(client, 'PUT').mockResolvedValue({
      data: null,
      error: { detail: 'update failed' },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    await expect(firstValueFrom(service.updatePaymentRequestByUser(1, {} as any))).rejects.toThrow(
      'update failed',
    );
  });
});
