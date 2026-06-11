import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  CreateSeasonRequestDto,
  ProblemDetails,
  SeasonDto,
  UpdateSeasonRequestDto,
} from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class SeasonService {
  public getSeasons(): Observable<SeasonDto[]> {
    const promise: Promise<SeasonDto[]> = client
      .GET('/api/v1/season', {})
      .then(({ data, error }) => {
        if (error) throw new Error(SeasonService.getErrorMessage(error));
        return data ?? [];
      });
    return from(promise);
  }

  public createSeason(request: CreateSeasonRequestDto): Observable<SeasonDto> {
    const promise: Promise<SeasonDto> = client
      .POST('/api/v1/season', { body: request })
      .then(({ data, error }) => {
        if (error) throw new Error(SeasonService.getErrorMessage(error));
        if (!data) throw new Error('No data returned');
        return data;
      });
    return from(promise);
  }

  public updateSeason(id: number, request: UpdateSeasonRequestDto): Observable<SeasonDto> {
    const promise: Promise<SeasonDto> = client
      .PUT('/api/v1/season/{id}', { params: { path: { id } }, body: request })
      .then(({ data, error }) => {
        if (error) throw new Error(SeasonService.getErrorMessage(error));
        if (!data) throw new Error('No data returned');
        return data;
      });
    return from(promise);
  }

  public deleteSeason(id: number): Observable<SeasonDto | null> {
    const promise: Promise<SeasonDto | null> = client
      .DELETE('/api/v1/season/{id}', { params: { path: { id } } })
      .then(({ data, error }) => {
        if (error) throw new Error(SeasonService.getErrorMessage(error));
        return data ?? null;
      });
    return from(promise);
  }

  private static getErrorMessage(error: unknown): string {
    return (error as ProblemDetails | undefined)?.detail ?? 'Unexpected Error';
  }
}
