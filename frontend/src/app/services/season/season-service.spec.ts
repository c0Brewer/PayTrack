//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import { CreateSeasonRequestDto, SeasonDto, UpdateSeasonRequestDto } from '../../types/exporter';

import { SeasonService } from './season-service';

const mockSeason: SeasonDto = {
  id: 1,
  name: '2026',
  isActive: true,
  budgets: [],
};

describe('SeasonService', () => {
  let service: SeasonService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SeasonService);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getSeasons', () => {
    it('should call API and return seasons', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: [mockSeason], error: null });

      const result = await firstValueFrom(service.getSeasons());

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).GET).toHaveBeenCalledWith('/api/v1/season', {
        params: { query: {} },
      });
      expect(result).toEqual([mockSeason]);
    });

    it('should pass query options to API', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: [mockSeason], error: null });

      await firstValueFrom(service.getSeasons({ IncludeInactive: true }));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).GET).toHaveBeenCalledWith('/api/v1/season', {
        params: { query: { IncludeInactive: true } },
      });
    });

    it('should return an empty list when API returns no data', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: undefined, error: null });

      const result = await firstValueFrom(service.getSeasons());

      expect(result).toEqual([]);
    });

    it('should throw error with detail when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Server error' },
      });

      await expect(firstValueFrom(service.getSeasons())).rejects.toThrow('Server error');
    });
  });

  describe('createSeason', () => {
    it('should call API and return created season', async () => {
      const request: CreateSeasonRequestDto = { name: '2026' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({ data: mockSeason, error: null });

      const result = await firstValueFrom(service.createSeason(request));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).POST).toHaveBeenCalledWith('/api/v1/season', { body: request });
      expect(result).toEqual(mockSeason);
    });

    it('should throw when API returns no created data', async () => {
      const request: CreateSeasonRequestDto = { name: '2026' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({ data: undefined, error: null });

      await expect(firstValueFrom(service.createSeason(request))).rejects.toThrow(
        'No data returned',
      );
    });
  });

  describe('updateSeason', () => {
    it('should call API and return updated season', async () => {
      const request: UpdateSeasonRequestDto = { name: '2027' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({ data: mockSeason, error: null });

      const result = await firstValueFrom(service.updateSeason(1, request));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).PUT).toHaveBeenCalledWith('/api/v1/season/{id}', {
        params: { path: { id: 1 } },
        body: request,
      });
      expect(result).toEqual(mockSeason);
    });

    it('should throw error when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({
        data: null,
        error: { detail: 'Not found' },
      });

      await expect(firstValueFrom(service.updateSeason(99, {}))).rejects.toThrow('Not found');
    });
  });

  describe('deleteSeason', () => {
    it('should call API and return null when hard-deleted', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ data: undefined, error: null });

      const result = await firstValueFrom(service.deleteSeason(1));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).DELETE).toHaveBeenCalledWith('/api/v1/season/{id}', {
        params: { path: { id: 1 } },
      });
      expect(result).toBeNull();
    });

    it('should return deactivated season when delete soft-deletes', async () => {
      const deactivatedSeason = { ...mockSeason, isActive: false };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ data: deactivatedSeason, error: null });

      const result = await firstValueFrom(service.deleteSeason(1));

      expect(result).toEqual(deactivatedSeason);
    });

    it('should throw default error when error has no detail', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ error: {} });

      await expect(firstValueFrom(service.deleteSeason(1))).rejects.toThrow('Unexpected Error');
    });
  });
});
