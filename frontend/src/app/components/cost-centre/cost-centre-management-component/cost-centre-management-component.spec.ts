import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  CostCentreDto,
  CostCentreDtoPaginatedResponse,
  DeleteCostCentrePreviewDto,
  GetCostCentreOptions,
  UpsertBudgetEntryDto,
} from '../../../types/exporter';
import { CostCentreSaveEvent } from '../../../types/misc-types';

import { CostCentreManagementComponent } from './cost-centre-management-component';

const mockCostCentres: CostCentreDto[] = [
  { id: 1, name: 'Aerodynamics', description: 'Aero costs', displayColor: '#FF5733', budgets: [], isActive: true },
  { id: 2, name: 'Powertrain', description: null, displayColor: null, budgets: [], isActive: true },
];

const mockPaginatedResponse: CostCentreDtoPaginatedResponse = {
  items: mockCostCentres,
  totalCount: 2,
  limit: 10,
  offset: 0,
  hasNext: false,
  hasPrevious: false,
};

const mockPreview: DeleteCostCentrePreviewDto = {
  costCentreName: 'Aerodynamics',
  budgetCount: 0,
  transactionCount: 0,
  affectedUserCount: 0,
  affectedTeamNames: [],
};

describe('CostCentreManagementComponent', () => {
  let component: CostCentreManagementComponent;
  let fixture: ComponentFixture<CostCentreManagementComponent>;
  let costCentreServiceMock: {
    getCostCentres: ReturnType<typeof vi.fn>;
    createCostCentre: ReturnType<typeof vi.fn>;
    updateCostCentre: ReturnType<typeof vi.fn>;
    getDeletePreview: ReturnType<typeof vi.fn>;
    deleteCostCentre: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
    showSuccess: ReturnType<typeof vi.fn>;
  };
  let cdrMock: { markForCheck: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    costCentreServiceMock = {
      getCostCentres: vi.fn().mockReturnValue(of(mockPaginatedResponse)),
      createCostCentre: vi.fn().mockReturnValue(of(mockCostCentres[0])),
      updateCostCentre: vi.fn().mockReturnValue(of(mockCostCentres[0])),
      getDeletePreview: vi.fn().mockReturnValue(of(mockPreview)),
      deleteCostCentre: vi.fn().mockReturnValue(of(null)),
    };
    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };
    cdrMock = { markForCheck: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [CostCentreManagementComponent],
      providers: [
        provideRouter([]),
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreManagementComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should load cost centres', () => {
    component.ngOnInit();
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
    expect(component.costCentres).toEqual(mockCostCentres);
  });

  it('load should show error when API throws', () => {
    costCentreServiceMock.getCostCentres.mockReturnValueOnce(
      throwError(() => new Error('API error')),
    );
    component.load();
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('openCreate should set editingCostCentre with id -1', () => {
    component.openCreate();
    expect(component.editingCostCentre).not.toBeNull();
    expect(component.editingCostCentre!.id).toBe(-1);
    expect(component.editingCostCentre!.name).toBe('');
  });

  it('openEdit should set editingCostCentre to a clone of the given cost centre', () => {
    component.openEdit(mockCostCentres[0]);
    expect(component.editingCostCentre).toEqual(mockCostCentres[0]);
    expect(component.editingCostCentre).not.toBe(mockCostCentres[0]);
  });

  it('closeEdit should reset editingCostCentre', () => {
    component.editingCostCentre = { ...mockCostCentres[0] };
    component.closeEdit();
    expect(component.editingCostCentre).toBeNull();
  });

  it('save with id -1 should call createCostCentre', () => {
    const event: CostCentreSaveEvent = {
      costCentre: { id: -1, name: 'New CC', description: null, displayColor: null, budgets: [], isActive: true },
      budgetsToUpsert: [],
      budgetIdsToDelete: [],
    };
    component.save(event);
    expect(costCentreServiceMock.createCostCentre).toHaveBeenCalledWith({
      name: 'New CC',
      description: undefined,
      displayColor: undefined,
      budgets: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
    expect(component.editingCostCentre).toBeNull();
  });

  it('save with existing id should call updateCostCentre', () => {
    const event: CostCentreSaveEvent = {
      costCentre: { ...mockCostCentres[0], name: 'Updated' },
      budgetsToUpsert: [],
      budgetIdsToDelete: [],
    };
    component.save(event);
    expect(costCentreServiceMock.updateCostCentre).toHaveBeenCalledWith(1, {
      name: 'Updated',
      description: 'Aero costs',
      displayColor: '#FF5733',
      budgetsToUpsert: undefined,
      budgetIdsToDelete: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
    expect(component.editingCostCentre).toBeNull();
  });

  it('save create should show error when API throws', () => {
    costCentreServiceMock.createCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Create failed')),
    );
    const event: CostCentreSaveEvent = {
      costCentre: { id: -1, name: 'X', description: null, displayColor: null, budgets: [], isActive: true },
      budgetsToUpsert: [],
      budgetIdsToDelete: [],
    };
    component.save(event);
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('save update should show error when API throws', () => {
    costCentreServiceMock.updateCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Update failed')),
    );
    const event: CostCentreSaveEvent = {
      costCentre: mockCostCentres[0],
      budgetsToUpsert: [],
      budgetIdsToDelete: [],
    };
    component.save(event);
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('openDelete should call getDeletePreview and set state', () => {
    component.openDelete(mockCostCentres[0]);
    expect(costCentreServiceMock.getDeletePreview).toHaveBeenCalledWith(1);
    expect(component.deletingCostCentre).toEqual(mockCostCentres[0]);
    expect(component.deletePreview).toEqual(mockPreview);
  });

  it('openDelete should show error when API throws', () => {
    costCentreServiceMock.getDeletePreview.mockReturnValueOnce(
      throwError(() => new Error('Preview failed')),
    );
    component.openDelete(mockCostCentres[0]);
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('closeDelete should reset delete state', () => {
    component.deletingCostCentre = mockCostCentres[0];
    component.deletePreview = mockPreview;
    component.closeDelete();
    expect(component.deletingCostCentre).toBeNull();
    expect(component.deletePreview).toBeNull();
  });

  it('confirmDelete should hard-delete and show "deleted successfully" when service returns null', () => {
    component.deletingCostCentre = mockCostCentres[0];
    component.confirmDelete();
    expect(costCentreServiceMock.deleteCostCentre).toHaveBeenCalledWith(1);
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      expect.stringContaining('deleted successfully'),
    );
    expect(component.deletingCostCentre).toBeNull();
  });

  it('confirmDelete should soft-delete and show "deactivated" when service returns a CostCentreDto', () => {
    const deactivated: CostCentreDto = { ...mockCostCentres[0], isActive: false };
    costCentreServiceMock.deleteCostCentre.mockReturnValueOnce(of(deactivated));
    component.deletingCostCentre = mockCostCentres[0];
    component.confirmDelete();
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      expect.stringContaining('deactivated'),
    );
    expect(component.deletingCostCentre).toBeNull();
  });

  it('confirmDelete should do nothing when deletingCostCentre is null', () => {
    component.deletingCostCentre = null;
    component.confirmDelete();
    expect(costCentreServiceMock.deleteCostCentre).not.toHaveBeenCalled();
  });

  it('confirmDelete should show error when API throws', () => {
    costCentreServiceMock.deleteCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Delete failed')),
    );
    component.deletingCostCentre = mockCostCentres[0];
    component.confirmDelete();
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('load should show error when response has no items', () => {
    costCentreServiceMock.getCostCentres.mockReturnValueOnce(
      of({ items: null, totalCount: 0, limit: 10, offset: 0 }),
    );
    component.load();
    expect(notificationServiceMock.showError).toHaveBeenCalledWith('Error while loading items');
  });

  it('updateFilterOptions should update filter state and reload', () => {
    const options: GetCostCentreOptions = {
      Name: 'Aero',
      Description: 'test',
      MinBudget: 100,
      MaxBudget: 500,
      Limit: undefined,
      Offset: undefined,
    };
    component.page = 3;
    component.updateFilterOptions(options);
    expect(component.filterOptions!.Name).toBe('Aero');
    expect(component.filterOptions!.Description).toBe('test');
    expect(component.filterOptions!.MinBudget).toBe(100);
    expect(component.filterOptions!.MaxBudget).toBe(500);
    expect(component.page).toBe(0);
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
  });

  it('getTotalPages should return correct page count', () => {
    component.totalCount = 25;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(3);
  });

  it('getTotalPages should return 1 when totalCount is 0', () => {
    component.totalCount = 0;
    component.limit = 10;
    expect(component.getTotalPages()).toBe(1);
  });

  it('onLimitChange should update limit, reset page to 0, and reload', () => {
    component.page = 2;
    component.onLimitChange(25);
    expect(component.limit).toBe(25);
    expect(component.page).toBe(0);
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
  });

  it('nextPage should increment page and reload', () => {
    component.page = 1;
    component.nextPage();
    expect(component.page).toBe(2);
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
  });

  it('previousPage should decrement page and reload when page is positive', () => {
    component.page = 2;
    component.previousPage();
    expect(component.page).toBe(1);
    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
  });

  it('previousPage should not decrement page when already at 0', () => {
    component.page = 0;
    const callsBefore = costCentreServiceMock.getCostCentres.mock.calls.length;
    component.previousPage();
    expect(component.page).toBe(0);
    expect(costCentreServiceMock.getCostCentres.mock.calls.length).toBe(callsBefore);
  });

  it('save create should include mapped budgets when budgetsToUpsert is not empty', () => {
    const budget: UpsertBudgetEntryDto = {
      id: undefined,
      teamId: 5,
      targetAmount: 1000,
      periodStart: '2024-01-01',
      periodEnd: '2024-12-31',
    };
    const event: CostCentreSaveEvent = {
      costCentre: { id: -1, name: 'New CC', description: null, displayColor: null, budgets: [], isActive: true },
      budgetsToUpsert: [budget],
      budgetIdsToDelete: [],
    };
    component.save(event);
    expect(costCentreServiceMock.createCostCentre).toHaveBeenCalledWith(
      expect.objectContaining({
        budgets: [
          { teamId: 5, targetAmount: 1000, periodStart: '2024-01-01', periodEnd: '2024-12-31' },
        ],
      }),
    );
  });

  it('save update should include budgetsToUpsert and budgetIdsToDelete when not empty', () => {
    const budget: UpsertBudgetEntryDto = {
      id: 10,
      teamId: 3,
      targetAmount: 200,
      periodStart: '2024-01-01',
      periodEnd: '2024-06-30',
    };
    const event: CostCentreSaveEvent = {
      costCentre: mockCostCentres[0],
      budgetsToUpsert: [budget],
      budgetIdsToDelete: [99],
    };
    component.save(event);
    expect(costCentreServiceMock.updateCostCentre).toHaveBeenCalledWith(
      1,
      expect.objectContaining({ budgetsToUpsert: [budget], budgetIdsToDelete: [99] }),
    );
  });
});
