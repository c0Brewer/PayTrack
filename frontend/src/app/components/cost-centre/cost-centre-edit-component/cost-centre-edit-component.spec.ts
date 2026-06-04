import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto, UpsertBudgetEntryDto } from '../../../types/exporter';

import { CostCentreEditComponent } from './cost-centre-edit-component';

describe('CostCentreEditComponent', () => {
  let component: CostCentreEditComponent;
  let fixture: ComponentFixture<CostCentreEditComponent>;
  let costCentreServiceMock: {
    createCostCentre: ReturnType<typeof vi.fn>;
    updateCostCentre: ReturnType<typeof vi.fn>;
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
    budgets: [
      {
        id: 10,
        name: 'Budget',
        description: null,
        teamId: 3,
        costCentreId: 1,
        seasonId: 1,
        targetAmount: 200,
        periodStart: '2024-01-01',
        periodEnd: '2024-06-30',
        type: 0,
        transactionIds: [],
        paidAmount: 0,
        approvedAmount: 0,
      },
    ],
    isActive: true,
  };

  beforeEach(async () => {
    costCentreServiceMock = {
      createCostCentre: vi.fn().mockReturnValue(of(mockCostCentre)),
      updateCostCentre: vi.fn().mockReturnValue(of(mockCostCentre)),
    };
    notificationServiceMock = {
      showError: vi.fn(),
      showSuccess: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [CostCentreEditComponent],
      providers: [
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreEditComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnChanges should clone the input cost centre and prepare budgets', () => {
    component.costCentre = mockCostCentre;

    component.ngOnChanges();

    expect(component.editingCostCentre).toEqual(mockCostCentre);
    expect(component.editingCostCentre).not.toBe(mockCostCentre);
    expect(component.workingBudgets).toEqual([
      {
        originalId: 10,
        name: 'Budget',
        seasonId: 1,
        teamId: 3,
        targetAmount: 200,
        periodStart: '2024-01-01',
        periodEnd: '2024-06-30',
        markedForDeletion: false,
      },
    ]);
  });

  it('saveEdit should close without update when existing cost centre has not changed', () => {
    const emitSpy = vi.spyOn(component.closeEvent, 'emit');
    component.costCentre = mockCostCentre;
    component.ngOnChanges();

    component.saveEdit();

    expect(costCentreServiceMock.updateCostCentre).not.toHaveBeenCalled();
    expect(emitSpy).toHaveBeenCalled();
  });

  it('saveEdit should call updateCostCentre for changed existing cost centre', () => {
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');
    component.costCentre = mockCostCentre;
    component.ngOnChanges();
    component.editingCostCentre!.name = 'Updated';

    component.saveEdit();

    expect(costCentreServiceMock.updateCostCentre).toHaveBeenCalledWith(1, {
      name: 'Updated',
      description: 'Aero costs',
      displayColor: '#FF5733',
      budgetsToUpsert: undefined,
      budgetIdsToDelete: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Cost centre updated successfully',
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('saveEdit should call createCostCentre for new cost centre', () => {
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');
    component.costCentre = {
      id: -1,
      name: 'New CC',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    };
    component.ngOnChanges();

    component.saveEdit();

    expect(costCentreServiceMock.createCostCentre).toHaveBeenCalledWith({
      name: 'New CC',
      description: undefined,
      displayColor: undefined,
      budgets: undefined,
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Cost centre created successfully',
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('saveEdit should include mapped budgets when creating', () => {
    component.costCentre = {
      id: -1,
      name: 'New CC',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    };
    component.ngOnChanges();
    component.newBudgets = [
      {
        id: undefined,
        name: 'Budget',
        description: null,
        teamId: 5,
        seasonId: 1,
        targetAmount: 1000,
        periodStart: '2024-01-01',
        periodEnd: '2024-12-31',
      },
    ];

    component.saveEdit();

    expect(costCentreServiceMock.createCostCentre).toHaveBeenCalledWith(
      expect.objectContaining({
        budgets: [
          {
            name: 'Budget',
            description: null,
            teamId: 5,
            seasonId: 1,
            targetAmount: 1000,
            periodStart: '2024-01-01T00:00:00.000Z',
            periodEnd: '2024-12-31T00:00:00.000Z',
          },
        ],
      }),
    );
  });

  it('saveEdit should include budgetsToUpsert and budgetIdsToDelete when updating', () => {
    const budget: UpsertBudgetEntryDto = {
      id: 10,
      name: 'Budget',
      description: null,
      teamId: 3,
      seasonId: 1,
      targetAmount: 200,
      periodStart: '2024-01-01',
      periodEnd: '2024-06-30',
    };
    component.costCentre = mockCostCentre;
    component.ngOnChanges();
    component.newBudgets = [budget];
    component.workingBudgets[0].markedForDeletion = true;

    component.saveEdit();

    expect(costCentreServiceMock.updateCostCentre).toHaveBeenCalledWith(
      1,
      expect.objectContaining({
        budgetsToUpsert: [
          {
            ...budget,
            id: null,
            periodStart: '2024-01-01T00:00:00.000Z',
            periodEnd: '2024-06-30T00:00:00.000Z',
          },
        ],
        budgetIdsToDelete: [10],
      }),
    );
  });

  it('saveEdit should show an error when create fails', () => {
    costCentreServiceMock.createCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Create failed')),
    );
    component.costCentre = {
      id: -1,
      name: 'New CC',
      description: null,
      displayColor: null,
      budgets: [],
      isActive: true,
    };
    component.ngOnChanges();

    component.saveEdit();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not create cost centre: Create failed',
    );
  });

  it('saveEdit should show an error when update fails', () => {
    costCentreServiceMock.updateCostCentre.mockReturnValueOnce(
      throwError(() => new Error('Update failed')),
    );
    component.costCentre = mockCostCentre;
    component.ngOnChanges();
    component.editingCostCentre!.name = 'Updated';

    component.saveEdit();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not update cost centre: Update failed',
    );
  });

  it('addNewBudget should add a complete draft and reset it', () => {
    component.newBudgetDraft = {
      id: null,
      name: 'Budget',
      description: null,
      teamId: 5,
      seasonId: 1,
      targetAmount: 100,
      periodStart: '2024-01-01',
      periodEnd: '2024-12-31',
    };

    component.addNewBudget();

    expect(component.newBudgets).toEqual([
      {
        id: null,
        name: 'Budget',
        description: null,
        teamId: 5,
        seasonId: 1,
        targetAmount: 100,
        periodStart: '2024-01-01',
        periodEnd: '2024-12-31',
      },
    ]);
    expect(component.newBudgetDraft).toEqual({
      id: null,
      name: '',
      description: null,
      teamId: 0,
      seasonId: 0,
      targetAmount: 0,
      periodStart: '',
      periodEnd: '',
    });
  });

  it('onClose should emit closeEvent', () => {
    const emitSpy = vi.spyOn(component.closeEvent, 'emit');

    component.onClose();

    expect(emitSpy).toHaveBeenCalled();
  });
});
