import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  CreatePaymentRequestByTeamDto,
  GetPaymentRequestsByTeamByIdOptions,
  GetPaymentRequestsByTeamOptions,
  PaginatedPaymentRequestByTeamDto,
  PaymentRequestByTeamDto,
} from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class PaymentRequestByTeamService {
  public getPaymentRequestsByTeam(
    queryOptions: GetPaymentRequestsByTeamOptions,
  ): Observable<PaginatedPaymentRequestByTeamDto> {
    const promise = client
      .GET('/api/v1/transaction/team', {
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

  public getPaymentRequestsByTeamById(
    id: number,
    queryOptions: GetPaymentRequestsByTeamByIdOptions,
  ): Observable<PaymentRequestByTeamDto> {
    const promise = client
      .GET('/api/v1/transaction/team/{id}', {
        params: {
          path: { id },
          query: queryOptions,
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public createPaymentRequestByTeam(
    payload: CreatePaymentRequestByTeamDto,
  ): Observable<PaymentRequestByTeamDto> {
    const promise = client
      .POST('/api/v1/transaction/team', { body: payload })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }
}
