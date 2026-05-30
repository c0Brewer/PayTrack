import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { client } from '../../client';
import { PaymentRequestByTeamDto } from '../../types/exporter';

import { PaymentRequestByTeamService } from './payment-request-by-team-service';

describe('PaymentRequestByTeamService', () => {
  let service: PaymentRequestByTeamService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PaymentRequestByTeamService],
    });
    service = TestBed.inject(PaymentRequestByTeamService);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  // -----------------------
  // GET LIST
  // -----------------------
  describe('getPaymentRequestsByTeam', () => {
    it('should return paginated list on success', async () => {
      const apiResponse = { items: [], totalCount: 0 };
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const result = await firstValueFrom(service.getPaymentRequestsByTeam({} as any));

      expect(client.GET).toHaveBeenCalledWith('/api/v1/transaction/team', {
        params: { query: {} },
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw on error response', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'list error' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        firstValueFrom(service.getPaymentRequestsByTeam({} as any)),
      ).rejects.toThrow('list error');
    });

    it('should throw "Unexpected Error" when error has no detail', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        firstValueFrom(service.getPaymentRequestsByTeam({} as any)),
      ).rejects.toThrow('Unexpected Error');
    });
  });

  // -----------------------
  // GET BY ID
  // -----------------------
  describe('getPaymentRequestsByTeamById', () => {
    it('should return the entity on success', async () => {
      const apiResponse = { id: 1, amount: 100 } as PaymentRequestByTeamDto;
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const result = await firstValueFrom(service.getPaymentRequestsByTeamById(1, {} as any));

      expect(client.GET).toHaveBeenCalledWith('/api/v1/transaction/team/{id}', {
        params: { path: { id: 1 }, query: {} },
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw on error response', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'not found' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        firstValueFrom(service.getPaymentRequestsByTeamById(99, {} as any)),
      ).rejects.toThrow('not found');
    });

    it('should throw "Unexpected Error" when error has no detail', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        firstValueFrom(service.getPaymentRequestsByTeamById(1, {} as any)),
      ).rejects.toThrow('Unexpected Error');
    });
  });

  // -----------------------
  // CREATE
  // -----------------------
  describe('createPaymentRequestByTeam', () => {
    it('should return created entity on success', async () => {
      const apiResponse = { id: 5, amount: 200 } as PaymentRequestByTeamDto;
      vi.spyOn(client, 'POST').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const result = await firstValueFrom(service.createPaymentRequestByTeam({} as any));

      expect(client.POST).toHaveBeenCalledWith('/api/v1/transaction/team', { body: {} });
      expect(result).toEqual(apiResponse);
    });

    it('should throw on error response', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        data: null,
        error: { detail: 'create failed' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        firstValueFrom(service.createPaymentRequestByTeam({} as any)),
      ).rejects.toThrow('create failed');
    });

    it('should throw "Unexpected Error" when error has no detail', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        firstValueFrom(service.createPaymentRequestByTeam({} as any)),
      ).rejects.toThrow('Unexpected Error');
    });
  });

  // -----------------------
  // MARK AS PAID
  // -----------------------
  describe('markAsPaid', () => {
    it('should return updated entity on success', async () => {
      const apiResponse = { id: 7, status: 3 } as PaymentRequestByTeamDto;
      vi.spyOn(client, 'POST').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.markAsPaid(7, { comment: 'done' }));

      expect(client.POST).toHaveBeenCalledWith('/api/v1/transaction/team/{id}/mark-as-paid', {
        params: { path: { id: 7 } },
        body: { comment: 'done' },
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw on error response', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        data: null,
        error: { detail: 'cannot mark as paid' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.markAsPaid(7, { comment: null }))).rejects.toThrow(
        'cannot mark as paid',
      );
    });

    it('should throw "Unexpected Error" when error has no detail', async () => {
      vi.spyOn(client, 'POST').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.markAsPaid(7, { comment: null }))).rejects.toThrow(
        'Unexpected Error',
      );
    });
  });
});
