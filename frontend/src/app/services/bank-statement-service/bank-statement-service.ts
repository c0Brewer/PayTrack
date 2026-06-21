import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  BankStatementEntryDto,
  BankStatementMatchResponseDto,
  BankStatementUpdateRequestDto,
  TransactionDto,
} from '../../types/exporter';
import { ensureOnlineForMutation } from '../offline/offline-utils';

@Injectable({
  providedIn: 'root',
})
export class BankStatementService {
  /**
   * POST — send raw bank statement entries and get back match candidates.
   */
  public getMatches(entries: BankStatementEntryDto[]): Observable<BankStatementMatchResponseDto> {
    ensureOnlineForMutation();

    const promise = client
      .POST('/api/v1/transaction/bank-statement-matches', {
        body: entries,
      })
      .then(({ data, error }) => {
        if (error)
          throw new Error(
            (error as { detail?: string }).detail ?? 'Failed to load bank statement matches',
          );
        if (!data) throw new Error('Empty response from bank statement matches');
        return data as BankStatementMatchResponseDto;
      });

    return from(promise);
  }

  /**
   * PUT — confirm which matches should be written back to the system.
   */
  public applyUpdates(updates: BankStatementUpdateRequestDto[]): Observable<TransactionDto[]> {
    ensureOnlineForMutation();

    const promise = client
      .PUT('/api/v1/transaction/bank-statement-matches', {
        body: updates,
      })
      .then(({ data, error }) => {
        if (error)
          throw new Error(
            (error as { detail?: string }).detail ?? 'Failed to apply bank statement updates',
          );
        if (!data) throw new Error('Empty response from bank statement update');
        return data as TransactionDto[];
      });

    return from(promise);
  }
}
