import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto, DeleteCostCentrePreviewDto } from '../../../types/exporter';

import { CostCentreDeleteComponent } from './cost-centre-delete-component';

describe('CostCentreDeleteComponent', () => {
  let component: CostCentreDeleteComponent;
  let fixture: ComponentFixture<CostCentreDeleteComponent>;
  let costCentreServiceMock: {
    getDeletePreview: ReturnType<typeof vi.fn>;
    deleteCostCentre: ReturnType<typeof vi.fn>;
  };
  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
    showSuccess: ReturnType<typeof vi.fn>;
  };

  const mockCostCentre: CostCentreDto = {
    id: 1,
    name: 'Aerodynamics',
    description: 'Aero costs',
    displayColor: '#FF5733',
    budgets: [],
    isActive: true,
  };

  const mockPreview: DeleteCostCentrePreviewDto = {
    costCentreName: 'Aerodynamics',
    budgetCount: 0,
    transactionCount: 0,
    affectedUserCount: 0,
    affectedTeamNames: [],
  };

  beforeEach(async () => {
    costCentreServiceMock = {
      getDeletePreview: vi.fn().mockReturnValue(of(mockPreview)),
      deleteCostCentre: vi.fn().mockReturnValue(of(null)),
    };
    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [CostCentreDeleteComponent],
      providers: [
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreDeleteComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnChanges should load delete preview for selected cost centre', () => {
    component.costCentre = mockCostCentre;

    component.ngOnChanges();

    expect(costCentreServiceMock.getDeletePreview).toHaveBeenCalledWith(1);
    expect(component.deletePreview).toEqual(mockPreview);
  });

  it('ngOnChanges should show error and close when preview loading fails', () => {
    const closeSpy = vi.spyOn(component.closeEvent, 'emit');
    costCentreServiceMock.getDeletePreview.mockReturnValueOnce(
      throwError(() => new Error('Preview failed')),
    );
    component.costCentre = mockCostCentre;

    component.ngOnChanges();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not load delete preview: Preview failed',
    );
    expect(closeSpy).toHaveBeenCalled();
  });

  it('hasLinkedDeleteRecords should detect linked budgets or transactions', () => {
    component.deletePreview = { ...mockPreview, budgetCount: 1 };
    expect(component.hasLinkedDeleteRecords).toBe(true);

    component.deletePreview = { ...mockPreview, budgetCount: 0, transactionCount: 1 };
    expect(component.hasLinkedDeleteRecords).toBe(true);

    component.deletePreview = mockPreview;
    expect(component.hasLinkedDeleteRecords).toBe(false);
  });

  it('confirmDelete should hard-delete and emit deleteEvent', () => {
    const emitSpy = vi.spyOn(component.deleteEvent, 'emit');
    component.costCentre = mockCostCentre;

    component.confirmDelete();

    expect(costCentreServiceMock.deleteCostCentre).toHaveBeenCalledWith(1);
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      expect.stringContaining('deleted successfully'),
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('confirmDelete should soft-delete and show deactivated when service returns a cost centre', () => {
    const emitSpy = vi.spyOn(component.deleteEvent, 'emit');
    costCentreServiceMock.deleteCostCentre.mockReturnValueOnce(
      of({ ...mockCostCentre, isActive: false }),
    );
    component.costCentre = mockCostCentre;

    component.confirmDelete();

    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      expect.stringContaining('deactivated'),
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('confirmDelete should do nothing without a selected cost centre', () => {
    component.costCentre = null;

    component.confirmDelete();

    expect(costCentreServiceMock.deleteCostCentre).not.toHaveBeenCalled();
  });

  it('confirmDelete should show error when delete fails', () => {
    const emitSpy = vi.spyOn(component.deleteEvent, 'emit');
    costCentreServiceMock.deleteCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Delete failed')),
    );
    component.costCentre = mockCostCentre;

    component.confirmDelete();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not delete cost centre: Delete failed',
    );
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('onClose should emit closeEvent', () => {
    const emitSpy = vi.spyOn(component.closeEvent, 'emit');

    component.onClose();

    expect(emitSpy).toHaveBeenCalled();
  });
});
