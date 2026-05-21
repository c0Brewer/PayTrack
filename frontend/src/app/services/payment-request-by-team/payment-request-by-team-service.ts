import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import { CreatePaymentRequestByTeamDto, PaymentRequestByTeamDto } from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class PaymentRequestByTeamService {
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
