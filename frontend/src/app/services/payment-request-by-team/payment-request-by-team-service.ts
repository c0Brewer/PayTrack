import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { client } from '../../client';
import {
  CreatePaymentRequestByTeamDto,
  GetPaymentRequestsByTeamByIdOptions,
  GetPaymentRequestsByTeamOptions,
  MarkAsPaidPaymentRequestByTeamDto,
  PaginatedPaymentRequestByTeamDto,
  PaymentRequestByTeamDto,
} from '../../types/exporter';
import { ensureOnlineForMutation, withOfflineReadFallback } from '../offline/offline-utils';

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

    return from(withOfflineReadFallback(promise));
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

    return from(withOfflineReadFallback(promise));
  }

  public createPaymentRequestByTeam(
    payload: CreatePaymentRequestByTeamDto,
  ): Observable<PaymentRequestByTeamDto> {
    ensureOnlineForMutation();

    const promise = client
      .POST('/api/v1/transaction/team', { body: payload })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public markAsPaid(
    id: number,
    payload: MarkAsPaidPaymentRequestByTeamDto,
  ): Observable<PaymentRequestByTeamDto> {
    ensureOnlineForMutation();

    const promise = client
      .POST('/api/v1/transaction/team/{id}/mark-as-paid', {
        params: { path: { id } },
        body: payload,
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        if (!data) throw new Error('Unexpected Error');
        return data;
      });

    return from(promise);
  }

  public deletePaymentRequestByTeam(id: number, reason?: string | null): Observable<void> {
    const token = localStorage.getItem('jwt');
    const promise = fetch(`${environment.apiBaseUrl}/api/v1/transaction/team/${id}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${token ?? ''}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ reason: reason ?? null }),
    }).then(async (res) => {
      if (!res.ok) {
        const err = (await res.json()) as { detail?: string };
        throw new Error(err.detail ?? 'Unexpected Error');
      }
    });

    return from(promise);
  }
}
