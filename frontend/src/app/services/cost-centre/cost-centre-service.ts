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

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type ApiResult = { data: any; error: any };

@Injectable({
  providedIn: 'root',
})
export class CostCentreService {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private readonly api = client as any;

  public getCostCentres(options?: GetCostCentreOptions): Observable<CostCentreDtoPaginatedResponse> {
    const promise: Promise<CostCentreDtoPaginatedResponse> = this.api
      .GET('/api/v1/cost-centre', { params: { query: options ?? {} } })
      .then(({ data, error }: ApiResult) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data as CostCentreDtoPaginatedResponse;
      });
    return from(promise);
  }

  public getCostCentre(id: number): Observable<CostCentreDto> {
    const promise: Promise<CostCentreDto> = this.api
      .GET('/api/v1/cost-centre/{id}', { params: { path: { id } } })
      .then(({ data, error }: ApiResult) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data as CostCentreDto;
      });
    return from(promise);
  }

  public createCostCentre(request: CreateCostCentreRequestDto): Observable<CostCentreDto> {
    const promise: Promise<CostCentreDto> = this.api
      .POST('/api/v1/cost-centre', { body: request })
      .then(({ data, error }: ApiResult) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data as CostCentreDto;
      });
    return from(promise);
  }

  public updateCostCentre(
    id: number,
    request: UpdateCostCentreRequestDto,
  ): Observable<CostCentreDto> {
    const promise: Promise<CostCentreDto> = this.api
      .PUT('/api/v1/cost-centre/{id}', { params: { path: { id } }, body: request })
      .then(({ data, error }: ApiResult) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data as CostCentreDto;
      });
    return from(promise);
  }

  public getDeletePreview(id: number): Observable<DeleteCostCentrePreviewDto> {
    const promise: Promise<DeleteCostCentrePreviewDto> = this.api
      .GET('/api/v1/cost-centre/{id}/delete-preview', { params: { path: { id } } })
      .then(({ data, error }: ApiResult) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data as DeleteCostCentrePreviewDto;
      });
    return from(promise);
  }

  public deleteCostCentre(id: number): Observable<void> {
    const promise: Promise<void> = this.api
      .DELETE('/api/v1/cost-centre/{id}', { params: { path: { id } } })
      .then(({ error }: ApiResult) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
      });
    return from(promise);
  }
}
