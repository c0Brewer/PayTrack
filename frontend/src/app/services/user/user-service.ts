import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';

export interface BankAccountRequestDto {
  accountHolder: string;
  iban: string;
  bic: string;
}

export interface BankAccountDto extends BankAccountRequestDto {
  id: number;
}

export interface BankAccountsResponseDto {
  bankAccounts: BankAccountDto[];
}

interface ApiErrorResponse {
  detail?: string;
}

interface ApiResponse<TData> {
  data?: TData;
  error?: ApiErrorResponse;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiClient = client as any;

  public getBankAccounts(): Observable<BankAccountsResponseDto> {
    const promise: Promise<BankAccountsResponseDto> = this.apiClient
      .GET('/api/v1/bankaccount', {
        params: {},
      })
      .then((response: ApiResponse<BankAccountsResponseDto>) => {
        this.throwIfError(response.error, 'Failed to load bank accounts');
        return this.toOverview(response.data);
      });

    return from(promise);
  }

  public createBankAccount(request: BankAccountRequestDto): Observable<BankAccountDto> {
    const promise: Promise<BankAccountDto> = this.apiClient
      .POST('/api/v1/bankaccount', {
        params: {},
        body: request,
      })
      .then((response: ApiResponse<BankAccountDto>) => {
        this.throwIfError(response.error, 'Failed to create bank account');
        return this.toBankAccount(response.data);
      });

    return from(promise);
  }

  public updateBankAccount(id: number, request: BankAccountRequestDto): Observable<BankAccountDto> {
    const promise: Promise<BankAccountDto> = this.apiClient
      .PUT('/api/v1/bankaccount/{id}', {
        params: {
          path: { id },
        },
        body: request,
      })
      .then((response: ApiResponse<BankAccountDto>) => {
        this.throwIfError(response.error, 'Failed to update bank account');
        return this.toBankAccount(response.data);
      });

    return from(promise);
  }

  public deleteBankAccount(id: number): Observable<void> {
    const promise: Promise<void> = this.apiClient
      .DELETE('/api/v1/bankaccount/{id}', {
        params: {
          path: { id },
        },
      })
      .then((response: ApiResponse<undefined>) => {
        this.throwIfError(response.error, 'Failed to delete bank account');
      });

    return from(promise);
  }

  private throwIfError(error: ApiErrorResponse | undefined, fallbackMessage: string): void {
    if (error) {
      throw new Error(error.detail ?? fallbackMessage);
    }
  }

  private toBankAccount(data: BankAccountDto | undefined): BankAccountDto {
    if (!data) {
      throw new Error('Bank account response is empty');
    }

    return {
      id: data.id,
      accountHolder: data.accountHolder,
      iban: data.iban,
      bic: data.bic,
    };
  }

  private toOverview(data: BankAccountsResponseDto | undefined): BankAccountsResponseDto {
    return {
      bankAccounts: data?.bankAccounts ?? [],
    };
  }
}
