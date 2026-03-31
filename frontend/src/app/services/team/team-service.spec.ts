import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { client } from '../../client';
import { TeamDto } from '../../types/exporter';

import { TeamService } from './team-service';

describe('TeamService', () => {
  let service: TeamService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TeamService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call API and return data for getTeams', async () => {
    const apiResponse: TeamDto[] = [
      { id: 123, name: '123', description: 'desc', displayColor: '#123456' },
    ];

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.getTeams());

    expect(client.GET).toHaveBeenCalledWith('/api/v1/team', {
      params: {},
    });

    expect(result).toEqual(apiResponse);
  });

  it('should call API and return error if error occurs', async () => {
    const error = {
      detail: 'An error occured',
    };

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: null,
      error: error,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(firstValueFrom(service.getTeams())).rejects.toThrow(error.detail);
  });

  it('should call API and return error if error occurs', async () => {
    const error = {};

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: null,
      error: error,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(firstValueFrom(service.getTeams())).rejects.toThrow('Unexpected Error');
  });
});
