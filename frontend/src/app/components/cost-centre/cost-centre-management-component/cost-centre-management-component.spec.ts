import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto, DeleteCostCentrePreviewDto } from '../../../types/exporter';

import { CostCentreManagementComponent } from './cost-centre-management-component';

const mockCostCentres: CostCentreDto[] = [
  { id: 1, name: 'Aerodynamics', description: 'Aero costs', displayColor: '#FF5733', budgets: [] },
  { id: 2, name: 'Powertrain', description: null, displayColor: null, budgets: [] },
];

const mockPreview: DeleteCostCentrePreviewDto = {
  costCentreName: 'Aerodynamics',
  budgetCount: 0,
  transactionCount: 0,
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
      getCostCentres: vi.fn().mockReturnValue(of(mockCostCentres)),
      createCostCentre: vi.fn().mockReturnValue(of(mockCostCentres[0])),
      updateCostCentre: vi.fn().mockReturnValue(of(mockCostCentres[0])),
      getDeletePreview: vi.fn().mockReturnValue(of(mockPreview)),
      deleteCostCentre: vi.fn().mockReturnValue(of(undefined)),
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
    const newCc: CostCentreDto = {
      id: -1,
      name: 'New CC',
      description: null,
      displayColor: null,
      budgets: [],
    };
    component.save(newCc);
    expect(costCentreServiceMock.createCostCentre).toHaveBeenCalledWith({
      name: 'New CC',
      description: undefined,
      displayColor: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
    expect(component.editingCostCentre).toBeNull();
  });

  it('save with existing id should call updateCostCentre', () => {
    const cc: CostCentreDto = { ...mockCostCentres[0], name: 'Updated' };
    component.save(cc);
    expect(costCentreServiceMock.updateCostCentre).toHaveBeenCalledWith(1, {
      name: 'Updated',
      description: 'Aero costs',
      displayColor: '#FF5733',
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
    expect(component.editingCostCentre).toBeNull();
  });

  it('save create should show error when API throws', () => {
    costCentreServiceMock.createCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Create failed')),
    );
    component.save({ id: -1, name: 'X', description: null, displayColor: null, budgets: [] });
    expect(notificationServiceMock.showError).toHaveBeenCalled();
  });

  it('save update should show error when API throws', () => {
    costCentreServiceMock.updateCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Update failed')),
    );
    component.save(mockCostCentres[0]);
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

  it('confirmDelete should call deleteCostCentre and reload', () => {
    component.deletingCostCentre = mockCostCentres[0];
    component.confirmDelete();
    expect(costCentreServiceMock.deleteCostCentre).toHaveBeenCalledWith(1);
    expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
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
});
