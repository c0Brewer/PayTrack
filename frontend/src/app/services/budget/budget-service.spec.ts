import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { client } from '../../client';
import {
  BudgetDto,
  BudgetDtoPaginatedResponse,
  CreateBudgetRequestDto,
  UpdateBudgetRequestDto,
} from '../../types/exporter';

import { BudgetService } from './budget-service';

describe('BudgetService', () => {
  let service: BudgetService;

  const budget = {
    id: 7,
    name: 'Operations 2026',
    teamId: 3,
  } as BudgetDto;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BudgetService);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should get budgets with query options', async () => {
    const response = { items: [budget], totalCount: 1 } as BudgetDtoPaginatedResponse;
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: response,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.getBudgets({ TeamId: 3 }));

    expect(client.GET).toHaveBeenCalledWith('/api/v1/budget', {
      params: { query: { TeamId: 3 } },
    });
    expect(result).toEqual(response);
  });

  it('should use an empty query when getting budgets without options', async () => {
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: { items: [] },
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await firstValueFrom(service.getBudgets());

    expect(client.GET).toHaveBeenCalledWith('/api/v1/budget', {
      params: { query: {} },
    });
  });

  it('should expose get budgets errors and reject missing data', async () => {
    vi.spyOn(client, 'GET')
      .mockResolvedValueOnce({
        data: null,
        error: { detail: 'Could not load budgets' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any)
      .mockResolvedValueOnce({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

    await expect(firstValueFrom(service.getBudgets())).rejects.toThrow('Could not load budgets');
    await expect(firstValueFrom(service.getBudgets())).rejects.toThrow('No data returned');
  });

  it('should get a budget by id', async () => {
    vi.spyOn(client, 'GET').mockResolvedValue({
      data: budget,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.getBudget(7));

    expect(client.GET).toHaveBeenCalledWith('/api/v1/budget/{id}', {
      params: { path: { id: 7 } },
    });
    expect(result).toEqual(budget);
  });

  it('should expose get budget errors and reject missing data', async () => {
    vi.spyOn(client, 'GET')
      .mockResolvedValueOnce({
        data: null,
        error: {},
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any)
      .mockResolvedValueOnce({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

    await expect(firstValueFrom(service.getBudget(7))).rejects.toThrow('Unexpected Error');
    await expect(firstValueFrom(service.getBudget(7))).rejects.toThrow('No data returned');
  });

  it('should create a budget', async () => {
    const request = { name: 'Operations 2026', teamId: 3 } as CreateBudgetRequestDto;
    vi.spyOn(client, 'POST').mockResolvedValue({
      data: budget,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.createBudget(request));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/budget', { body: request });
    expect(result).toEqual(budget);
  });

  it('should expose create budget errors and reject missing data', async () => {
    const request = {} as CreateBudgetRequestDto;
    vi.spyOn(client, 'POST')
      .mockResolvedValueOnce({
        data: null,
        error: { detail: 'Invalid budget' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any)
      .mockResolvedValueOnce({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

    await expect(firstValueFrom(service.createBudget(request))).rejects.toThrow('Invalid budget');
    await expect(firstValueFrom(service.createBudget(request))).rejects.toThrow('No data returned');
  });

  it('should update a budget', async () => {
    const request = { name: 'Updated budget' } as UpdateBudgetRequestDto;
    vi.spyOn(client, 'PUT').mockResolvedValue({
      data: budget,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.updateBudget(7, request));

    expect(client.PUT).toHaveBeenCalledWith('/api/v1/budget/{id}', {
      params: { path: { id: 7 } },
      body: request,
    });
    expect(result).toEqual(budget);
  });

  it('should expose update budget errors and reject missing data', async () => {
    const request = {} as UpdateBudgetRequestDto;
    vi.spyOn(client, 'PUT')
      .mockResolvedValueOnce({
        data: null,
        error: { detail: 'Budget not found' },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any)
      .mockResolvedValueOnce({
        data: null,
        error: null,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

    await expect(firstValueFrom(service.updateBudget(7, request))).rejects.toThrow(
      'Budget not found',
    );
    await expect(firstValueFrom(service.updateBudget(7, request))).rejects.toThrow(
      'No data returned',
    );
  });

  it('should delete a budget', async () => {
    vi.spyOn(client, 'DELETE').mockResolvedValue({
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(firstValueFrom(service.deleteBudget(7))).resolves.toBeUndefined();
    expect(client.DELETE).toHaveBeenCalledWith('/api/v1/budget/{id}', {
      params: { path: { id: 7 } },
    });
  });

  it('should expose delete budget errors', async () => {
    vi.spyOn(client, 'DELETE').mockResolvedValue({
      error: { detail: 'Budget is in use' },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(firstValueFrom(service.deleteBudget(7))).rejects.toThrow('Budget is in use');
  });
});
