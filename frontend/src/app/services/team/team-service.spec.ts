import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { client } from '../../client';
import {
  CreateTeamRequestDto,
  DeleteTeamImpactDto,
  TeamDto,
  TeamDtoPaginatedResponse,
  UpdateTeamDto,
} from '../../types/exporter';

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

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('getTeams', () => {
    it('should call the API with query options and return paginated teams', async () => {
      const queryOptions = {
        Name: 'Platform',
        IncludeMembers: true,
        Limit: 10,
        Offset: 0,
      };

      const apiResponse: TeamDtoPaginatedResponse = {
        items: [{ id: 123, name: 'Platform', description: 'desc', displayColor: '#123456' }],
        totalCount: 1,
        limit: 10,
        offset: 0,
        hasNext: false,
        hasPrevious: false,
      };

      vi.spyOn(client, 'GET').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.getTeams(queryOptions));

      expect(client.GET).toHaveBeenCalledWith('/api/v1/team', {
        params: { query: queryOptions },
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw the backend error detail when getTeams fails with a specific error', async () => {
      const error = { detail: 'An error occured' };

      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getTeams({}))).rejects.toThrow(error.detail);
    });

    it('should throw the default error message when getTeams fails without a detail', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getTeams({}))).rejects.toThrow('Unexpected Error');
    });

    it('should throw when getTeams resolves without data and without an API error', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getTeams({}))).rejects.toThrow('No data returned');
    });
  });

  describe('getTeamById', () => {
    it('should call the API and return a single team', async () => {
      const teamId = 42;
      const apiResponse: TeamDto = {
        id: teamId,
        name: 'Operations',
        description: 'Runs production',
        displayColor: '#334155',
      };

      vi.spyOn(client, 'GET').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.getTeamById(teamId));

      expect(client.GET).toHaveBeenCalledWith('/api/v1/team/{id}', {
        params: {
          path: {
            id: teamId,
          },
          query: {},
        },
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw the backend error detail when getTeamById fails with a specific error', async () => {
      const error = { detail: 'Team not found' };

      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getTeamById(42))).rejects.toThrow(error.detail);
    });

    it('should throw the default error message when getTeamById fails without a detail', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getTeamById(42))).rejects.toThrow('Unexpected Error');
    });

    it('should throw when getTeamById resolves without data and without an API error', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getTeamById(42))).rejects.toThrow('No data returned');
    });
  });

  describe('updateTeam', () => {
    it('should call API and return the updated team', async () => {
      const updateRequest: UpdateTeamDto = {
        name: 'Updated Platform',
        description: 'Updated description',
        displayColor: '#0f172a',
      };
      const apiResponse: TeamDto = {
        id: 42,
        name: 'Updated Platform',
        description: 'Updated description',
        displayColor: '#0f172a',
      };

      vi.spyOn(client, 'PUT').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.updateTeam(42, updateRequest));

      expect(client.PUT).toHaveBeenCalledWith('/api/v1/team/{id}', {
        params: {
          path: {
            id: 42,
          },
        },
        body: updateRequest,
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw the backend error detail when updateTeam fails with a specific error', async () => {
      const updateRequest: UpdateTeamDto = { name: 'Updated Platform' };
      const error = { detail: 'Update failed' };

      vi.spyOn(client, 'PUT').mockResolvedValue({
        data: null,
        error,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.updateTeam(42, updateRequest))).rejects.toThrow(
        error.detail,
      );
    });

    it('should throw the default error message when updateTeam fails without a detail', async () => {
      const updateRequest: UpdateTeamDto = { name: 'Updated Platform' };

      vi.spyOn(client, 'PUT').mockResolvedValue({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.updateTeam(42, updateRequest))).rejects.toThrow(
        'Unexpected Error',
      );
    });
  });

  describe('createTeam', () => {
    it('should call API and return the created team', async () => {
      const createRequest: CreateTeamRequestDto = {
        name: 'New Team',
        description: 'Freshly created',
        displayColor: '#2563eb',
      };
      const apiResponse: TeamDto = {
        id: 99,
        name: 'New Team',
        description: 'Freshly created',
        displayColor: '#2563eb',
      };

      vi.spyOn(client, 'POST').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.createTeam(createRequest));

      expect(client.POST).toHaveBeenCalledWith('/api/v1/team', {
        body: createRequest,
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw the backend error detail when createTeam fails with a specific error', async () => {
      const createRequest: CreateTeamRequestDto = { name: 'New Team' };
      const error = { detail: 'Create failed' };

      vi.spyOn(client, 'POST').mockResolvedValue({
        data: null,
        error,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.createTeam(createRequest))).rejects.toThrow(error.detail);
    });

    it('should throw when createTeam resolves without data and without an API error', async () => {
      const createRequest: CreateTeamRequestDto = { name: 'New Team' };

      vi.spyOn(client, 'POST').mockResolvedValue({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.createTeam(createRequest))).rejects.toThrow(
        'No data returned',
      );
    });
  });

  describe('getDeleteImpact', () => {
    it('should call API and return delete impact', async () => {
      const apiResponse: DeleteTeamImpactDto = {
        teamId: 42,
        teamName: 'Platform',
        canDelete: false,
        affectedUserCount: 2,
        blockingBudgetCount: 1,
        blockingTransactionCount: 3,
        invoiceCount: 0,
        warningMessage: 'Deleting this team has impact.',
      };

      vi.spyOn(client, 'GET').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.getDeleteImpact(42));

      expect(client.GET).toHaveBeenCalledWith('/api/v1/team/{id}/delete-impact', {
        params: {
          path: {
            id: 42,
          },
        },
      });
      expect(result).toEqual(apiResponse);
    });

    it('should throw the backend error detail when getDeleteImpact fails', async () => {
      vi.spyOn(client, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Impact failed' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.getDeleteImpact(42))).rejects.toThrow('Impact failed');
    });
  });

  describe('deleteTeam', () => {
    it('should call API and return null when the team is deleted', async () => {
      vi.spyOn(client, 'DELETE').mockResolvedValue({
        data: undefined,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const result = await firstValueFrom(service.deleteTeam(42));

      expect(client.DELETE).toHaveBeenCalledWith('/api/v1/team/{id}', {
        params: {
          path: {
            id: 42,
          },
        },
      });
      expect(result).toBeNull();
    });

    it('should return the team when the delete endpoint deactivates it', async () => {
      const apiResponse: TeamDto = {
        id: 42,
        name: 'Platform',
        description: null,
        displayColor: '#2563eb',
        isActive: false,
      };

      vi.spyOn(client, 'DELETE').mockResolvedValue({
        data: apiResponse,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.deleteTeam(42))).resolves.toEqual(apiResponse);
    });

    it('should throw the backend error detail when deleteTeam fails', async () => {
      vi.spyOn(client, 'DELETE').mockResolvedValue({
        data: null,
        error: { detail: 'Delete failed' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      await expect(firstValueFrom(service.deleteTeam(42))).rejects.toThrow('Delete failed');
    });
  });
});
