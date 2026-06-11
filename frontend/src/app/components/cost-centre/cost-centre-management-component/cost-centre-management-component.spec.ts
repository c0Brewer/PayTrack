import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { SeasonService } from '../../../services/season/season-service';
import { TeamService } from '../../../services/team/team-service';
import {
  CostCentreDto,
  CostCentreDtoPaginatedResponse,
  GetCostCentreOptions,
  SeasonDto,
  TeamDtoPaginatedResponse,
} from '../../../types/exporter';

import { CostCentreManagementComponent } from './cost-centre-management-component';

const mockCostCentres: CostCentreDto[] = [
  {
    id: 1,
    name: 'Aerodynamics',
    description: 'Aero costs',
    displayColor: '#FF5733',
    budgets: [],
    isActive: true,
  },
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

const mockSeasons: SeasonDto[] = [
  { id: 1, name: '2025', isActive: true, budgets: [] },
  { id: 2, name: '2026', isActive: true, budgets: [] },
];
const mockTeams: TeamDtoPaginatedResponse = {
  items: [{ id: 1, name: 'Platform', description: null, displayColor: null, members: [] }],
  totalCount: 1,
  limit: 1000,
  offset: 0,
  hasNext: false,
  hasPrevious: false,
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
  let seasonServiceMock: {
    getSeasons: ReturnType<typeof vi.fn>;
  };
  let teamServiceMock: {
    getTeams: ReturnType<typeof vi.fn>;
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
      getDeletePreview: vi.fn(),
      deleteCostCentre: vi.fn().mockReturnValue(of(null)),
    };
    seasonServiceMock = {
      getSeasons: vi.fn().mockReturnValue(of(mockSeasons)),
    };
    teamServiceMock = {
      getTeams: vi.fn().mockReturnValue(of(mockTeams)),
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
        { provide: SeasonService, useValue: seasonServiceMock },
        { provide: TeamService, useValue: teamServiceMock },
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
    expect(teamServiceMock.getTeams).toHaveBeenCalled();
    expect(seasonServiceMock.getSeasons).toHaveBeenCalledWith({ IncludeInactive: true });
    expect(component.costCentres).toEqual(mockCostCentres);
    expect(component.seasons).toEqual(mockSeasons);
  });

  it('load should show error when API throws', () => {
    costCentreServiceMock.getCostCentres.mockReturnValueOnce(
      throwError(() => new Error('API error')),
    );
    component.load();
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('loadSeasons should show error when API throws', () => {
    seasonServiceMock.getSeasons.mockReturnValueOnce(throwError(() => new Error('Season failed')));

    component.loadSeasons();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load seasons: Season failed',
    );
  });

  it('openCreate should set editingCostCentre with id -1', () => {
    component.openCreate();
    expect(component.editingCostCentre).not.toBeNull();
    expect(component.editingCostCentre!.id).toBe(-1);
    expect(component.editingCostCentre!.name).toBe('');
  });

  it('openEdit should set editingCostCentre to the given cost centre', () => {
    component.openEdit(mockCostCentres[0]);
    expect(component.editingCostCentre).toEqual(mockCostCentres[0]);
    expect(component.editingCostCentre).toBe(mockCostCentres[0]);
  });

  it('closeEdit should reset editingCostCentre', () => {
    component.editingCostCentre = { ...mockCostCentres[0] };
    component.closeEdit();
    expect(component.editingCostCentre).toBeNull();
  });

  it('onCostCentreSaved should reload and close edit modal', () => {
    component.editingCostCentre = { ...mockCostCentres[0] };
    component.onCostCentreSaved();

    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
    expect(component.editingCostCentre).toBeNull();
  });

  it('openDelete should set deletingCostCentre', () => {
    component.openDelete(mockCostCentres[0]);
    expect(component.deletingCostCentre).toEqual(mockCostCentres[0]);
  });

  it('closeDelete should reset deletingCostCentre', () => {
    component.deletingCostCentre = mockCostCentres[0];
    component.closeDelete();
    expect(component.deletingCostCentre).toBeNull();
  });

  it('onCostCentreDeleted should reload and close delete modal', () => {
    component.deletingCostCentre = mockCostCentres[0];
    component.onCostCentreDeleted();

    expect(costCentreServiceMock.getCostCentres).toHaveBeenCalled();
    expect(component.deletingCostCentre).toBeNull();
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
});
