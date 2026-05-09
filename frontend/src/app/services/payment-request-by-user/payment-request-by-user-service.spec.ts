import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

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

  afterEach(() => {
    vi.restoreAllMocks();
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

    expect(globalThis.fetch).toHaveBeenCalled();
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

  it('should mark payment request as paid', async () => {
    const apiResponse = { id: 1, paymentReference: 'REF-1' } as PaymentRequestByUserDto;
    const request = {
      paymentReference: 'REF-1',
      purposeOfPayment: 'Supplier payout',
      paymentDate: '2026-02-03T00:00:00.000Z',
    };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.markPaymentRequestByUserAsPaid(1, request));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/transaction/user/{id}/mark-paid', {
      params: { path: { id: 1 } },
      body: request,
    });
    expect(result).toEqual(apiResponse);
  });

  it('should approve payment request', async () => {
    const apiResponse = { id: 1 } as PaymentRequestByUserDto;
    const request = { costCentreId: 5, reason: 'ok' };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.approvePaymentRequestByUser(1, request));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/transaction/user/{id}/approve', {
      params: { path: { id: 1 } },
      body: request,
    });
    expect(result).toEqual(apiResponse);
  });

  it('should decline payment request', async () => {
    const apiResponse = { id: 1 } as PaymentRequestByUserDto;
    const request = { reason: 'duplicate' };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.declinePaymentRequestByUser(1, request));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/transaction/user/{id}/decline', {
      params: { path: { id: 1 } },
      body: request,
    });
    expect(result).toEqual(apiResponse);
  });

  it('should request changes for payment request', async () => {
    const apiResponse = { id: 1 } as PaymentRequestByUserDto;
    const request = { reason: 'missing receipt' };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.requestChangesForPaymentRequestByUser(1, request));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/transaction/user/{id}/request-changes', {
      params: { path: { id: 1 } },
      body: request,
    });
    expect(result).toEqual(apiResponse);
  });

  it('should throw API detail for status update failures', async () => {
    vi.spyOn(client, 'POST').mockResolvedValue({
      data: null,
      error: { detail: 'invalid status' },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(
      firstValueFrom(service.declinePaymentRequestByUser(1, { reason: 'duplicate' })),
    ).rejects.toThrow('invalid status');
  });

  it('should download receipt as blob', async () => {
    const receipt = new Blob(['receipt'], { type: 'application/pdf' });

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: receipt,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.downloadReceipt(1));

    expect(client.GET).toHaveBeenCalledWith('/api/v1/transaction/user/{id}/receipt', {
      params: { path: { id: 1 } },
      parseAs: 'blob',
    });
    expect(result).toBe(receipt);
  });
});
