import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../../client';
import {
  CreateTeamRequestDto,
  DeleteTeamImpactDto,
  TeamDto,
  GetTeamByIdOptions,
  GetTeamOptions,
  TeamDtoPaginatedResponse,
  UpdateTeamDto,
} from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  public getTeams(queryOptions: GetTeamOptions): Observable<TeamDtoPaginatedResponse> {
    const promise = client
      .GET('/api/v1/team', {
        params: {
          query: queryOptions,
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(promise);
  }

  public getTeamById(teamId: number, options?: GetTeamByIdOptions): Observable<TeamDto> {
    const promise = client
      .GET('/api/v1/team/{id}', {
        params: {
          path: {
            id: teamId,
          },
          query: options ?? {},
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(promise);
  }

  public createTeam(createRequest: CreateTeamRequestDto): Observable<TeamDto> {
    const promise = client
      .POST('/api/v1/team', {
        body: createRequest,
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(promise);
  }

  public updateTeam(teamId: number, updateRequest: UpdateTeamDto): Observable<TeamDto> {
    const promise = client
      .PUT('/api/v1/team/{id}', {
        params: {
          path: {
            id: teamId,
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

  public getDeleteImpact(teamId: number): Observable<DeleteTeamImpactDto> {
    const promise = client
      .GET('/api/v1/team/{id}/delete-impact', {
        params: {
          path: {
            id: teamId,
          },
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        if (!data) throw new Error('No data returned');
        return data;
      });

    return from(promise);
  }

  public deleteTeam(teamId: number): Observable<TeamDto | null> {
    const promise = client
      .DELETE('/api/v1/team/{id}', {
        params: {
          path: {
            id: teamId,
          },
        },
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data ?? null;
      });

    return from(promise);
  }
}
