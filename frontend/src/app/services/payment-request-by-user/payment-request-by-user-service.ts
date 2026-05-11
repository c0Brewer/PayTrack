import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { client } from '../../client';
import {
  CreatePaymentRequestByUserDto,
  GetPaymentRequestsByUserByIdOptions,
  GetPaymentRequestsByUserOptions,
  PaginatedPaymentRequestByUserDto,
  PaymentRequestByUserDto,
  UpdatePaymentRequestByUserDto,
} from '../../types/exporter';
import { AuthService } from '../auth/auth-service';

@Injectable({
  providedIn: 'root',
})
export class PaymentRequestByUserService {
  constructor(private readonly authService: AuthService) {}

  public getPaymentRequestsByUser(
    queryOptions: GetPaymentRequestsByUserOptions,
  ): Observable<PaginatedPaymentRequestByUserDto> {
    const promise = client
      .GET('/api/v1/transaction/user', {
        params: {
          query: queryOptions,
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public getPaymentRequestsByUserById(
    id: number,
    queryOptions: GetPaymentRequestsByUserByIdOptions,
  ): Observable<PaymentRequestByUserDto> {
    const promise = client
      .GET('/api/v1/transaction/user/{id}', {
        params: {
          path: {
            id: id,
          },
          query: queryOptions,
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public createPaymentRequestByUser(
    updateRequest: CreatePaymentRequestByUserDto,
    file: File,
  ): Observable<PaymentRequestByUserDto> {
    // DISCLAIMER: This method is intentionally not using the client for requests.
    // This is not the standard and is only needed for this method because of file
    // upload. Do not copy!!

    const fd = new FormData();

    fd.append('receipt', file);
    fd.append('invoiceNumber', updateRequest.invoiceNumber);
    fd.append('comment', updateRequest.comment ?? '');
    fd.append('payoutType', String(updateRequest.payoutType));
    fd.append('bankAccountId', String(updateRequest.bankAccountId));
    fd.append('transaction.teamId', String(updateRequest.transaction.teamId));
    fd.append('transaction.amount', String(updateRequest.transaction.amount));
    fd.append('transaction.purposeOfPayment', updateRequest.transaction.purposeOfPayment);
    fd.append('transaction.paidAt', updateRequest.transaction.paidAt);

    const uploadUrl = new URL('/api/v1/transaction/user', environment.apiBaseUrl).toString();
    const promise = fetch(uploadUrl, {
      method: 'POST',
      headers: {
        // NOTE: do NOT set Content-Type here — browser sets it with the boundary
        Authorization: `Bearer ${this.authService.getToken()}`, // however you handle auth
      },
      body: fd,
    }).then(async (res) => {
      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.detail ?? 'Unexpected Error');
      }
      return res.json() as Promise<PaymentRequestByUserDto>;
    });

    return from(promise);
  }

  public updatePaymentRequestByUser(
    id: number,
    updateRequest: UpdatePaymentRequestByUserDto,
  ): Observable<PaymentRequestByUserDto> {
    const promise = client
      .PUT('/api/v1/transaction/user/{id}', {
        params: {
          path: {
            id: id,
          },
        },
        body: updateRequest,
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public downloadReceipt(id: number): Observable<Blob> {
    const promise = client
      .GET('/api/v1/transaction/user/{id}/receipt', {
        params: { path: { id } },
        parseAs: 'blob',
      })
      .then(({ data, error }) => {
        if (error) throw new Error('Unexpected Error');
        return data as Blob;
      });

    return from(promise);
  }
}
