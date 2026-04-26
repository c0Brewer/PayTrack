//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import {
  BankAccountDto,
  CreateBankAccountRequestDto,
  UpdateBankAccountRequestDto,
} from '../../types/exporter';

import { BankAccountService } from './bank-account-service';

describe('BankAccountService', () => {
  let service: BankAccountService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BankAccountService);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getBankAccounts', () => {
    it('should return bank accounts on success', async () => {
      const response: BankAccountDto[] = [
        { id: 1, accountHolder: 'Max', iban: 'AT611904300234573201', bic: 'BKAUATWW' },
      ];

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.getBankAccounts());

      expect(client.GET).toHaveBeenCalledWith('/api/v1/bankaccount', {
        params: {},
      });
      expect(result).toEqual(response);
    });

    it('should return empty array when data is null', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: null, error: null } as any);

      const result = await firstValueFrom(service.getBankAccounts());
      expect(result).toEqual([]);
    });

    it('should throw when api returns error', async () => {
      const error = { detail: 'Failed to load bank accounts' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: null, error } as any);

      await expect(firstValueFrom(service.getBankAccounts())).rejects.toThrow(
        'Failed to load bank accounts',
      );
    });
  });

  describe('createBankAccount', () => {
    it('should return created bank account on success', async () => {
      const request: CreateBankAccountRequestDto = {
        accountHolder: 'Max',
        iban: 'AT611904300234573201',
        bic: 'BKAUATWW',
      };
      const response: BankAccountDto = { id: 4, ...request };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'POST').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.createBankAccount(request));

      expect(client.POST).toHaveBeenCalledWith('/api/v1/bankaccount', {
        params: {},
        body: request,
      });
      expect(result).toEqual(response);
    });

    it('should throw when api returns error', async () => {
      const request: CreateBankAccountRequestDto = {
        accountHolder: 'Max',
        iban: 'AT611904300234573201',
        bic: 'BKAUATWW',
      };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'POST').mockResolvedValue({ data: null, error: {} } as any);

      await expect(firstValueFrom(service.createBankAccount(request))).rejects.toThrow(
        'Failed to create bank account',
      );
    });
  });

  describe('updateBankAccount', () => {
    it('should return updated bank account on success', async () => {
      const request: UpdateBankAccountRequestDto = {
        accountHolder: 'Updated',
        iban: 'AT611904300234573202',
        bic: 'NEWBIC12',
      };
      const response: BankAccountDto = {
        id: 7,
        accountHolder: 'Updated',
        iban: 'AT611904300234573202',
        bic: 'NEWBIC12',
      };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ data: response, error: null } as any);

      const result = await firstValueFrom(service.updateBankAccount(7, request));

      expect(client.PUT).toHaveBeenCalledWith('/api/v1/bankaccount/{id}', {
        params: { path: { id: 7 } },
        body: request,
      });
      expect(result).toEqual(response);
    });

    it('should throw when api returns no data', async () => {
      const request: UpdateBankAccountRequestDto = { bic: 'NEWBIC12' };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ data: null, error: null } as any);

      await expect(firstValueFrom(service.updateBankAccount(7, request))).rejects.toThrow(
        'Failed to update bank account',
      );
    });
  });

  describe('deleteBankAccount', () => {
    it('should call delete endpoint and resolve on success', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'DELETE').mockResolvedValue({ error: null } as any);

      const result = await firstValueFrom(service.deleteBankAccount(9));

      expect(client.DELETE).toHaveBeenCalledWith('/api/v1/bankaccount/{id}', {
        params: { path: { id: 9 } },
      });
      expect(result).toBeUndefined();
    });

    it('should throw when api returns delete error', async () => {
      const error = { detail: 'Failed to delete bank account' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'DELETE').mockResolvedValue({ error } as any);

      await expect(firstValueFrom(service.deleteBankAccount(9))).rejects.toThrow(
        'Failed to delete bank account',
      );
    });
  });
});
