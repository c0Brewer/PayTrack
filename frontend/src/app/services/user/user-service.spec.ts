import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import { UserDto, UserDtoPaginatedResponse, UpdateUserDto } from '../../types/exporter';

import { UserService } from './user-service';

describe('UserService', () => {
  let service: UserService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserService);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getUser', () => {
    it('should call API and return paginated users', async () => {
      const mockResponse: UserDtoPaginatedResponse = {
        totalCount: 2,
        limit: 10,
        offset: 0,
        items: [
          {
            id: 1,
            name: 'Alice',
            email: 'a@test.com',
            role: 0,
            isActive: true,
            team: { id: 1, name: 'Team 1', description: '', displayColor: '' },
            profilePictureUrl: '',
          },
          {
            id: 2,
            name: 'Bob',
            email: 'b@test.com',
            role: 0,
            isActive: true,
            team: { id: 1, name: 'Team 1', description: '', displayColor: '' },
            profilePictureUrl: '',
          },
        ],
      };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: mockResponse, error: null } as any);

      const result = await firstValueFrom(service.getUser({ Limit: 10, Offset: 0 }));

      expect(client.GET).toHaveBeenCalledWith('/api/v1/user', {
        params: { query: { Limit: 10, Offset: 0 } },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should throw an error if API returns error', async () => {
      const error = { detail: 'Failed to fetch users' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: null, error } as any);

      await expect(firstValueFrom(service.getUser({ Limit: 10, Offset: 0 }))).rejects.toThrow(
        'Failed to fetch users',
      );
    });

    it('should throw default error if API returns empty error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'GET').mockResolvedValue({ data: null, error: {} } as any);

      await expect(firstValueFrom(service.getUser({ Limit: 10, Offset: 0 }))).rejects.toThrow(
        'Unexpected Error',
      );
    });
  });

  describe('updateUser', () => {
    it('should call API and return updated user', async () => {
      const updateRequest: UpdateUserDto = {
        name: 'Updated Name',
        role: 1,
        isActive: false,
        teamId: 2,
      };
      const mockUser: UserDto = {
        id: 1,
        name: 'Updated Name',
        email: 'a@test.com',
        role: 1,
        isActive: false,
        team: { id: 2, name: 'Team 2', description: '', displayColor: '' },
        profilePictureUrl: '',
      };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ data: mockUser, error: null } as any);

      const result = await firstValueFrom(service.updateUser(1, updateRequest));

      expect(client.PUT).toHaveBeenCalledWith('/api/v1/user/{id}', {
        params: { path: { id: 1 } },
        body: updateRequest,
      });
      expect(result).toEqual(mockUser);
    });

    it('should throw an error if API returns error', async () => {
      const updateRequest: UpdateUserDto = { name: 'Updated', role: 1, isActive: true, teamId: 1 };
      const error = { detail: 'Update failed' };

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ data: null, error } as any);

      await expect(firstValueFrom(service.updateUser(1, updateRequest))).rejects.toThrow(
        'Update failed',
      );
    });

    it('should throw default error if API returns empty error', async () => {
      const updateRequest: UpdateUserDto = { name: 'Updated', role: 1, isActive: true, teamId: 1 };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client, 'PUT').mockResolvedValue({ data: null, error: {} } as any);

      await expect(firstValueFrom(service.updateUser(1, updateRequest))).rejects.toThrow(
        'Unexpected Error',
      );
    });
  });
});
