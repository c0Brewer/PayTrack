import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  CostCentreDto,
  CostCentreDtoPaginatedResponse,
  CreateCostCentreRequestDto,
  DeleteCostCentrePreviewDto,
  GetCostCentreOptions,
  UpdateCostCentreRequestDto,
} from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class CostCentreService {
  public getCostCentres(
    options?: GetCostCentreOptions,
  ): Observable<CostCentreDtoPaginatedResponse> {
    const promise: Promise<CostCentreDtoPaginatedResponse> = client
      .GET('/api/v1/cost-centre', { params: { query: options ?? {} } })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });
    return from(promise);
  }

  public getCostCentre(id: number): Observable<CostCentreDto> {
    const promise: Promise<CostCentreDto> = client
      .GET('/api/v1/cost-centre/{id}', { params: { path: { id } } })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });
    return from(promise);
  }

  public createCostCentre(request: CreateCostCentreRequestDto): Observable<CostCentreDto> {
    const promise: Promise<CostCentreDto> = client
      .POST('/api/v1/cost-centre', { body: request })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });
    return from(promise);
  }

  public updateCostCentre(
    id: number,
    request: UpdateCostCentreRequestDto,
  ): Observable<CostCentreDto> {
    const promise: Promise<CostCentreDto> = client
      .PUT('/api/v1/cost-centre/{id}', { params: { path: { id } }, body: request })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });
    return from(promise);
  }

  public getDeletePreview(id: number): Observable<DeleteCostCentrePreviewDto> {
    const promise: Promise<DeleteCostCentrePreviewDto> = client
      .GET('/api/v1/cost-centre/{id}/delete-preview', { params: { path: { id } } })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });
    return from(promise);
  }

  public deleteCostCentre(id: number): Observable<CostCentreDto | null> {
    const promise: Promise<CostCentreDto | null> = client
      .DELETE('/api/v1/cost-centre/{id}', { params: { path: { id } } })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data ?? null;
      });
    return from(promise);
  }
}
