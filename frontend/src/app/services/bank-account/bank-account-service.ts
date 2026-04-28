import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  BankAccountDto,
  CreateBankAccountRequestDto,
  UpdateBankAccountRequestDto,
} from '../../types/exporter';

export type { BankAccountDto, CreateBankAccountRequestDto, UpdateBankAccountRequestDto };

@Injectable({
  providedIn: 'root',
})
export class BankAccountService {
  public getBankAccounts(): Observable<BankAccountDto[]> {
    const promise = client
      .GET('/api/v1/bankaccount', {
        params: {},
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to load bank accounts');
        return data;
      });

    return from(promise);
  }

  public createBankAccount(request: CreateBankAccountRequestDto): Observable<BankAccountDto> {
    const promise = client
      .POST('/api/v1/bankaccount', {
        params: {},
        body: request,
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to create bank account');
        if (!data) throw new Error('Failed to create bank account');
        return data;
      });

    return from(promise);
  }

  public updateBankAccount(
    id: number,
    request: UpdateBankAccountRequestDto,
  ): Observable<BankAccountDto> {
    const promise = client
      .PUT('/api/v1/bankaccount/{id}', {
        params: {
          path: { id },
        },
        body: request,
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to update bank account');
        if (!data) throw new Error('Failed to update bank account');
        return data;
      });

    return from(promise);
  }

  public deleteBankAccount(id: number): Observable<void> {
    const promise = client
      .DELETE('/api/v1/bankaccount/{id}', {
        params: {
          path: { id },
        },
      })
      .then(({ error }) => {
        if (error) throw new Error(error.detail ?? 'Failed to delete bank account');
      });

    return from(promise);
  }
}
