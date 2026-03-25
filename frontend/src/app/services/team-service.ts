import { Injectable } from '@angular/core';

import { client } from '../client';
import { TeamDto } from '../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  async getTeams(): Promise<TeamDto[]> {
    const { data, error } = await client.GET('/api/v1/team', {
      params: {},
    });

    if (error) throw new Error(error);
    return data;
  }
}
