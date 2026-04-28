import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import {
  CostCentreDto,
  CostCentreDtoPaginatedResponse,
  CreateCostCentreRequestDto,
  DeleteCostCentrePreviewDto,
  UpdateCostCentreRequestDto,
} from '../../types/exporter';

import { CostCentreService } from './cost-centre-service';

const mockCostCentre: CostCentreDto = {
  id: 1,
  name: 'Aerodynamics',
  description: 'Aero dept costs',
  displayColor: '#FF5733',
  budgets: [],
  isActive: true,
};

describe('CostCentreService', () => {
  let service: CostCentreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CostCentreService);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCostCentres', () => {
    it('should call API and return paginated cost centres', async () => {
      const mockPaginated: CostCentreDtoPaginatedResponse = {
        items: [mockCostCentre],
        totalCount: 1,
        limit: 10,
        offset: 0,
        hasNext: false,
        hasPrevious: false,
      };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: mockPaginated, error: null });

      const result = await firstValueFrom(service.getCostCentres());

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).GET).toHaveBeenCalledWith('/api/v1/cost-centre', {
        params: { query: {} },
      });
      expect(result).toEqual(mockPaginated);
    });

    it('should throw error with detail when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Server error' },
      });

      await expect(firstValueFrom(service.getCostCentres())).rejects.toThrow('Server error');
    });

    it('should throw default error when error has no detail', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: null, error: {} });

      await expect(firstValueFrom(service.getCostCentres())).rejects.toThrow('Unexpected Error');
    });
  });

  describe('getCostCentre', () => {
    it('should call API with id and return cost centre', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: mockCostCentre, error: null });

      const result = await firstValueFrom(service.getCostCentre(1));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).GET).toHaveBeenCalledWith('/api/v1/cost-centre/{id}', {
        params: { path: { id: 1 } },
      });
      expect(result).toEqual(mockCostCentre);
    });

    it('should throw error when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Not found' },
      });

      await expect(firstValueFrom(service.getCostCentre(99))).rejects.toThrow('Not found');
    });
  });

  describe('createCostCentre', () => {
    it('should call API and return created cost centre', async () => {
      const request: CreateCostCentreRequestDto = {
        name: 'Powertrain',
        description: 'Engine costs',
      };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({ data: mockCostCentre, error: null });

      const result = await firstValueFrom(service.createCostCentre(request));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).POST).toHaveBeenCalledWith('/api/v1/cost-centre', { body: request });
      expect(result).toEqual(mockCostCentre);
    });

    it('should throw error when API returns error', async () => {
      const request: CreateCostCentreRequestDto = { name: 'ab' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'POST').mockResolvedValue({
        data: null,
        error: { detail: 'Validation failed' },
      });

      await expect(firstValueFrom(service.createCostCentre(request))).rejects.toThrow(
        'Validation failed',
      );
    });
  });

  describe('updateCostCentre', () => {
    it('should call API and return updated cost centre', async () => {
      const request: UpdateCostCentreRequestDto = { name: 'Updated Name' };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({ data: mockCostCentre, error: null });

      const result = await firstValueFrom(service.updateCostCentre(1, request));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).PUT).toHaveBeenCalledWith('/api/v1/cost-centre/{id}', {
        params: { path: { id: 1 } },
        body: request,
      });
      expect(result).toEqual(mockCostCentre);
    });

    it('should throw error when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'PUT').mockResolvedValue({
        data: null,
        error: { detail: 'Not found' },
      });

      await expect(firstValueFrom(service.updateCostCentre(99, {}))).rejects.toThrow('Not found');
    });
  });

  describe('getDeletePreview', () => {
    it('should call API and return delete preview', async () => {
      const preview: DeleteCostCentrePreviewDto = {
        costCentreName: 'Aerodynamics',
        budgetCount: 3,
        transactionCount: 12,
        affectedUserCount: 5,
        affectedTeamNames: ['Aero Team'],
      };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({ data: preview, error: null });

      const result = await firstValueFrom(service.getDeletePreview(1));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).GET).toHaveBeenCalledWith('/api/v1/cost-centre/{id}/delete-preview', {
        params: { path: { id: 1 } },
      });
      expect(result).toEqual(preview);
    });

    it('should throw error when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'GET').mockResolvedValue({
        data: null,
        error: { detail: 'Not found' },
      });

      await expect(firstValueFrom(service.getDeletePreview(99))).rejects.toThrow('Not found');
    });
  });

  describe('deleteCostCentre', () => {
    it('should return null on 204 (hard delete)', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ data: undefined, error: null });

      const result = await firstValueFrom(service.deleteCostCentre(1));

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect((client as any).DELETE).toHaveBeenCalledWith('/api/v1/cost-centre/{id}', {
        params: { path: { id: 1 } },
      });
      expect(result).toBeNull();
    });

    it('should return CostCentreDto on 200 (soft delete)', async () => {
      const deactivated: CostCentreDto = { ...mockCostCentre, isActive: false };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ data: deactivated, error: null });

      const result = await firstValueFrom(service.deleteCostCentre(1));

      expect(result).toEqual(deactivated);
    });

    it('should throw error when API returns error', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ error: { detail: 'Not found' } });

      await expect(firstValueFrom(service.deleteCostCentre(1))).rejects.toThrow('Not found');
    });

    it('should throw default error when error has no detail', async () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      vi.spyOn(client as any, 'DELETE').mockResolvedValue({ error: {} });

      await expect(firstValueFrom(service.deleteCostCentre(1))).rejects.toThrow('Unexpected Error');
    });
  });
});
