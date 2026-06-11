import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { BudgetType, CostCentreDto, SeasonDto, TeamDto } from '../../../types/exporter';

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
  const mockSeasons: SeasonDto[] = [
    { id: 1, name: '2025', isActive: true, budgets: [] },
    { id: 2, name: '2026', isActive: false, budgets: [] },
  ];

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
      type: BudgetType.Expense,
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
        type: BudgetType.Expense,
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
    expect(component.newBudgetDraft).toEqual({
      id: null,
      name: '',
      description: null,
      teamId: 0,
      seasonId: 0,
      targetAmount: null,
      periodStart: '',
      periodEnd: '',
      type: BudgetType.Expense,
    });
  });

  it('should mark missing budget fields red with messages', () => {
    fixture.detectChanges(false);

    clickAddBudgetButton();
    fixture.detectChanges(false);

    expect(component.newBudgets).toEqual([]);
    expect(component.hasBudgetFieldError('name')).toBe(true);
    expect(component.hasBudgetFieldError('teamId')).toBe(true);
    expect(component.hasBudgetFieldError('targetAmount')).toBe(true);
    expect(component.hasBudgetFieldError('seasonId')).toBe(true);
    expect(component.hasBudgetFieldError('periodStart')).toBe(true);
    expect(component.hasBudgetFieldError('periodEnd')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Name is required.');
    expect(fixture.nativeElement.textContent).toContain('Team is required.');
    expect(fixture.nativeElement.textContent).toContain('Amount is required.');
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
      type: BudgetType.Expense,
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
      type: BudgetType.Expense,
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

  it('ngOnChanges should clone cost centre and map working budgets', () => {
    const costCentre: CostCentreDto = {
      id: 5,
      name: 'IT',
      description: null,
      displayColor: null,
      isActive: true,
      budgets: [
        {
          id: 10,
          name: 'Laptops',
          description: null,
          costCentreId: 5,
          teamId: 1,
          seasonId: 2,
          targetAmount: 1000,
          periodStart: '2026-01-01',
          periodEnd: '2026-12-31',
          type: BudgetType.Expense,
          transactionIds: [],
          paidAmount: 500,
          approvedAmount: 200,
        },
      ],
    };
    component.costCentre = costCentre;

    component.ngOnChanges();

    expect(component.originalCostCentre).toEqual(costCentre);
    expect(component.originalCostCentre).not.toBe(costCentre);
    expect(component.workingBudgets).toHaveLength(1);
    expect(component.workingBudgets[0].targetAmount).toBe(1000);
    expect(component.workingBudgets[0].type).toBe(BudgetType.Expense);
  });

  it('getBudgetFieldError should skip amount validation for Income budgets', () => {
    component.newBudgetDraft = {
      id: null,
      name: 'Merch sales',
      teamId: 1,
      seasonId: 1,
      targetAmount: null,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
      type: BudgetType.Income,
    };

    expect(component.getBudgetFieldError('targetAmount')).toBe('');
    expect(component.hasBudgetFieldError('targetAmount')).toBe(false);
  });

  it('getTeamOptionLabel should append inactive label for inactive teams', () => {
    component.teams = [
      { id: 1, name: 'Alpha', isActive: true },
      { id: 2, name: 'Beta', isActive: false },
    ] as unknown as TeamDto[];

    expect(component.getTeamOptionLabel(component.teams[0])).toBe('Alpha');
    expect(component.getTeamOptionLabel(component.teams[1])).toBe('Beta (inactive)');
  });

  it('getTeamName should return team name or fallback', () => {
    component.teams = [{ id: 3, name: 'Gamma' } as unknown as TeamDto];

    expect(component.getTeamName(3)).toBe('Gamma');
    expect(component.getTeamName(99)).toBe('Team #99');
  });

  it('getSeasonName should return season name or fallback', () => {
    component.seasons = mockSeasons;

    expect(component.getSeasonName(1)).toBe('2025');
    expect(component.getSeasonName(2)).toBe('2026');
    expect(component.getSeasonName(99)).toBe('Season #99');
  });

  it('should label inactive season options and prevent adding them to new budgets', () => {
    component.seasons = mockSeasons;
    component.teams = [{ id: 1, name: 'Team', isActive: true } as TeamDto];
    component.newBudgetDraft = {
      id: null,
      name: 'Blocked budget',
      teamId: 1,
      seasonId: 2,
      targetAmount: 500,
      periodStart: '2026-01-01',
      periodEnd: '2026-12-31',
      type: BudgetType.Expense,
    };

    component.addNewBudget();

    expect(component.getSeasonOptionLabel(mockSeasons[1])).toBe('2026 (inactive)');
    expect(component.isSeasonActive(1)).toBe(true);
    expect(component.isSeasonActive(2)).toBe(false);
    expect(component.newBudgets).toEqual([]);
  });

  it('isCreating should be true when id is -1 and false otherwise', () => {
    expect(component.isCreating).toBe(true);
    component.costCentre = { ...component.costCentre, id: 5 };
    expect(component.isCreating).toBe(false);
  });

  it('hasChanged should return true when name has changed', () => {
    component.costCentre = { ...component.costCentre, id: 1, name: 'New Name' };
    component.ngOnChanges();
    component.costCentre.name = 'Changed Name';

    expect(component.hasChanged()).toBe(true);
  });

  it('hasChanged should return false when nothing has changed', () => {
    component.costCentre = { ...component.costCentre, id: 1, name: 'Same' };
    component.ngOnChanges();

    expect(component.hasChanged()).toBe(false);
  });

  it('onSave should close when not creating and nothing has changed', () => {
    const closeSpy = vi.spyOn(component, 'onClose');
    component.costCentre = { ...component.costCentre, id: 1, name: 'Unchanged' };
    component.ngOnChanges();

    component.onSave();

    expect(closeSpy).toHaveBeenCalled();
    expect(costCentreServiceMock.updateCostCentre).not.toHaveBeenCalled();
  });

  it('should create a cost centre with budget data and normalized dates', () => {
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
        type: BudgetType.Expense,
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
          type: BudgetType.Expense,
        },
      ],
    });
    expect(notificationServiceMock.showSuccess).toHaveBeenCalledWith(
      'Cost centre created successfully',
    );
    expect(emitSpy).toHaveBeenCalled();
  });

  it('should update a cost centre with budget data and normalized dates', () => {
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
        type: BudgetType.Expense,
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
            type: BudgetType.Expense,
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

  it('toggleBudgetDeletion should flip markedForDeletion', () => {
    const budget = { markedForDeletion: false } as Parameters<
      typeof component.toggleBudgetDeletion
    >[0];

    component.toggleBudgetDeletion(budget);
    expect(budget.markedForDeletion).toBe(true);

    component.toggleBudgetDeletion(budget);
    expect(budget.markedForDeletion).toBe(false);
  });

  it('removeNewBudget should remove the budget at the given index', () => {
    component.newBudgets = [
      { ...component.newBudgetDraft, name: 'A' },
      { ...component.newBudgetDraft, name: 'B' },
    ];

    component.removeNewBudget(0);

    expect(component.newBudgets).toHaveLength(1);
    expect(component.newBudgets[0].name).toBe('B');
  });

  it('isTeamActive should handle active, inactive, and missing teams', () => {
    component.teams = [
      { id: 1, isActive: true },
      { id: 2, isActive: false },
    ] as unknown as TeamDto[];

    expect(component.isTeamActive(1)).toBe(true);
    expect(component.isTeamActive(2)).toBe(false);
    expect(component.isTeamActive(999)).toBe(true);
  });

  it('onBudgetFieldBlur should mark the field as touched', () => {
    component.onBudgetFieldBlur('name');
    expect(component.touchedBudgetFields['name']).toBe(true);
  });

  it('onClose should emit closeEvent', () => {
    const spy = vi.spyOn(component.closeEvent, 'emit');

    component.onClose();

    expect(spy).toHaveBeenCalled();
  });
});
