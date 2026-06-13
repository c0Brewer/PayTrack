import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  BudgetDto,
  BudgetDtoPaginatedResponse,
  CreateBudgetRequestDto,
  GetBudgetOptions,
  ProblemDetails,
  UpdateBudgetRequestDto,
} from '../../types/exporter';
import { ensureOnlineForMutation, withOfflineReadFallback } from '../offline/offline-utils';

@Injectable({
  providedIn: 'root',
})
export class BudgetService {
  public getBudgets(options?: GetBudgetOptions): Observable<BudgetDtoPaginatedResponse> {
    const promise: Promise<BudgetDtoPaginatedResponse> = client
      .GET('/api/v1/budget', { params: { query: options ?? {} } })
      .then(({ data, error }) => {
        if (error) throw new Error(BudgetService.getErrorMessage(error));
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(withOfflineReadFallback(promise));
  }

  public getBudget(id: number): Observable<BudgetDto> {
    const promise: Promise<BudgetDto> = client
      .GET('/api/v1/budget/{id}', { params: { path: { id } } })
      .then(({ data, error }) => {
        if (error) throw new Error(BudgetService.getErrorMessage(error));
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(withOfflineReadFallback(promise));
  }

  public createBudget(request: CreateBudgetRequestDto): Observable<BudgetDto> {
    ensureOnlineForMutation();

    const promise: Promise<BudgetDto> = client
      .POST('/api/v1/budget', { body: request })
      .then(({ data, error }) => {
        if (error) throw new Error(BudgetService.getErrorMessage(error));
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(promise);
  }

  public updateBudget(id: number, request: UpdateBudgetRequestDto): Observable<BudgetDto> {
    ensureOnlineForMutation();

    const promise: Promise<BudgetDto> = client
      .PUT('/api/v1/budget/{id}', { params: { path: { id } }, body: request })
      .then(({ data, error }) => {
        if (error) throw new Error(BudgetService.getErrorMessage(error));
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(promise);
  }

  public deleteBudget(id: number): Observable<void> {
    ensureOnlineForMutation();

    const promise: Promise<void> = client
      .DELETE('/api/v1/budget/{id}', { params: { path: { id } } })
      .then(({ error }) => {
        if (error) throw new Error(BudgetService.getErrorMessage(error));
      });

    return from(promise);
  }

  private static getErrorMessage(error: unknown): string {
    return (error as ProblemDetails | undefined)?.detail ?? 'Unexpected Error';
  }
}
