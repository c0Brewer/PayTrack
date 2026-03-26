import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { client } from '../client';
import { TeamDto } from '../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  public getTeams(): Observable<TeamDto[]> {
    const promise = client
      .GET('/api/v1/team', {
        params: {},
      })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });

    return from(promise);
  }
}
