import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { CostCentreDto } from '../../../types/exporter';

import { CostCentreEditModalComponent } from './cost-centre-edit-modal-component';

describe('CostCentreEditModalComponent', () => {
  let component: CostCentreEditModalComponent;
  let fixture: ComponentFixture<CostCentreEditModalComponent>;
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
    budgets: [],
    isActive: true,
  };

  function clickAddBudgetButton(): void {
    const addButton = fixture.nativeElement.querySelector(
      '.btn-add-budget',
    ) as HTMLButtonElement | null;

    addButton?.click();
  }

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
      imports: [CostCentreEditModalComponent],
      providers: [
        { provide: CostCentreService, useValue: costCentreServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCentreEditModalComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should add a valid budget and reset validation state', () => {
    component.newBudgetDraft = {
      id: null,
      name: 'Budget',
      description: null,
      teamId: 1,
      seasonId: 1,
      targetAmount: 500,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
    };

    component.addNewBudget();

    expect(component.newBudgets).toEqual([
      {
        id: null,
        name: 'Budget',
        description: null,
        teamId: 1,
        seasonId: 1,
        targetAmount: 500,
        periodStart: '2026-01-01',
        periodEnd: '2026-12-31',
      },
    ]);
    expect(component.touchedBudgetFields).toEqual({
      name: false,
      teamId: false,
      targetAmount: false,
      seasonId: false,
      periodStart: false,
      periodEnd: false,
    });
  });

  it('should mark missing budget fields red with messages', () => {
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('name')).toBe(true);
    expect(component.hasBudgetFieldError('teamId')).toBe(true);
    expect(component.hasBudgetFieldError('seasonId')).toBe(true);
    expect(component.hasBudgetFieldError('periodStart')).toBe(true);
    expect(component.hasBudgetFieldError('periodEnd')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Name is required.');
    expect(fixture.nativeElement.textContent).toContain('Team is required.');
    expect(fixture.nativeElement.textContent).toContain('Season is required.');
    expect(fixture.nativeElement.textContent).toContain('Period start is required.');
    expect(fixture.nativeElement.textContent).toContain('Period end is required.');

    const invalidControls = fixture.nativeElement.querySelectorAll('.input-error');
    expect(invalidControls.length).toBeGreaterThanOrEqual(5);
  });

  it('should mark a negative budget amount red with a message', () => {
    component.newBudgetDraft = {
      id: null,
      name: 'Budget',
      description: null,
      teamId: 1,
      seasonId: 1,
      targetAmount: -1,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
    };
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    const amountInput = fixture.nativeElement.querySelector(
      'input[placeholder="0.00"]',
    ) as HTMLInputElement;

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('targetAmount')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Amount must be non-negative.');
    expect(amountInput.classList).toContain('input-error');
  });

  it('should mark a budget period end before the start red with a message', () => {
    component.newBudgetDraft = {
      id: null,
      name: 'Budget',
      description: null,
      teamId: 1,
      seasonId: 1,
      targetAmount: 500,
      periodStart: '2026-12-31',
      periodEnd: '2026-01-01',
    };
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    const dateInputs = fixture.nativeElement.querySelectorAll(
      'input[type="date"]',
    ) as NodeListOf<HTMLInputElement>;

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('periodEnd')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain(
      'Period end must not be before period start.',
    );
    expect(dateInputs[1].classList).toContain('input-error');
  });

  it('should create a cost centre with season budget data and normalized dates', () => {
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
    component.newBudgets = [
      {
        id: null,
        name: 'Budget',
        description: null,
        teamId: 1,
        seasonId: 2,
        targetAmount: 500,
        periodStart: '2026-01-01',
        periodEnd: '2026-12-31',
      },
    ];

    component.onSave();

    expect(costCentreServiceMock.createCostCentre).toHaveBeenCalledWith({
      name: 'New CC',
      description: undefined,
      displayColor: undefined,
      budgets: [
        {
          name: 'Budget',
          description: null,
          teamId: 1,
          seasonId: 2,
          targetAmount: 500,
          periodStart: '2026-01-01T00:00:00.000Z',
          periodEnd: '2026-12-31T00:00:00.000Z',
        },
      ],
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Cost centre created successfully',
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('should update a cost centre with season budget data and normalized dates', () => {
    const emitSpy = vi.spyOn(component.saveEvent, 'emit');
    component.costCentre = mockCostCentre;
    component.ngOnChanges();
    component.newBudgets = [
      {
        id: 10,
        name: 'Updated budget',
        description: null,
        teamId: 1,
        seasonId: 2,
        targetAmount: 500,
        periodStart: '2026-01-01',
        periodEnd: '2026-12-31',
      },
    ];

    component.onSave();

    expect(costCentreServiceMock.updateCostCentre).toHaveBeenCalledWith(
      1,
      expect.objectContaining({
        budgetsToUpsert: [
          {
            id: null,
            name: 'Updated budget',
            description: null,
            teamId: 1,
            seasonId: 2,
            targetAmount: 500,
            periodStart: '2026-01-01T00:00:00.000Z',
            periodEnd: '2026-12-31T00:00:00.000Z',
          },
        ],
      }),
    );
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Cost centre updated successfully',
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('should show an error when saving fails', () => {
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

    component.onSave();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith(
      'Could not create cost centre: Create failed',
    );
  });
});
