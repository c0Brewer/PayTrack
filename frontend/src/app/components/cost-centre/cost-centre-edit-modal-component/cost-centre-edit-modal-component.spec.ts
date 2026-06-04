import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BudgetType, CostCentreDto, TeamDto } from '../../../types/exporter';

import { CostCentreEditModalComponent } from './cost-centre-edit-modal-component';

describe('CostCentreEditModalComponent', () => {
  let component: CostCentreEditModalComponent;
  let fixture: ComponentFixture<CostCentreEditModalComponent>;

  function clickAddBudgetButton(): void {
    const addButton = fixture.nativeElement.querySelector(
      '.btn-add-budget',
    ) as HTMLButtonElement | null;

    addButton?.click();
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CostCentreEditModalComponent],
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
          type: 0,
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
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component.teams = [{ id: 3, name: 'Gamma' } as any];

    expect(component.getTeamName(3)).toBe('Gamma');
    expect(component.getTeamName(99)).toBe('Team #99');
  });

  it('getSeasonName should return season name or fallback', () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component.seasons = [{ id: 1, name: '2025' } as any];

    expect(component.getSeasonName(1)).toBe('2025');
    expect(component.getSeasonName(99)).toBe('Season #99');
  });

  it('isCreating should be true when id is -1 and false otherwise', () => {
    expect(component.isCreating).toBe(true);
    component.costCentre = { ...component.costCentre, id: 5 };
    expect(component.isCreating).toBe(false);
  });

  describe('hasChanged', () => {
    it('should return false when originalCostCentre is null', () => {
      component.originalCostCentre = null;
      expect(component.hasChanged()).toBe(false);
    });

    it('should return true when name has changed', () => {
      component.costCentre = { ...component.costCentre, id: 1, name: 'New Name' };
      component.ngOnChanges();
      component.costCentre.name = 'Changed Name';
      expect(component.hasChanged()).toBe(true);
    });

    it('should return false when nothing has changed', () => {
      component.costCentre = { ...component.costCentre, id: 1, name: 'Same' };
      component.ngOnChanges();
      expect(component.hasChanged()).toBe(false);
    });

    it('should return true when newBudgets has entries', () => {
      component.costCentre = { ...component.costCentre, id: 1 };
      component.ngOnChanges();

      component.newBudgets = [
        {
          id: null,
          name: 'B',
          description: null,
          teamId: 1,
          seasonId: 1,
          targetAmount: 0,
          periodStart: '2026-01-01',
          periodEnd: '2026-12-31',
        },
      ];
      expect(component.hasChanged()).toBe(true);
    });
  });

  describe('onSave', () => {
    it('should call onClose when not creating and nothing has changed', () => {
      const closeSpy = vi.spyOn(component, 'onClose');
      component.costCentre = { ...component.costCentre, id: 1, name: 'Unchanged' };
      component.ngOnChanges();
      component.onSave();
      expect(closeSpy).toHaveBeenCalled();
    });

    it('should emit saveEvent when creating (id === -1)', () => {
      const emitSpy = vi.spyOn(component.saveEvent, 'emit');
      component.costCentre = { ...component.costCentre, id: -1 };
      component.onSave();
      expect(emitSpy).toHaveBeenCalledWith({
        costCentre: component.costCentre,
        budgetsToUpsert: [],
        budgetIdsToDelete: [],
      });
    });

    it('should emit saveEvent with correct budgets when not creating but changed', () => {
      const emitSpy = vi.spyOn(component.saveEvent, 'emit');
      component.costCentre = { ...component.costCentre, id: 2, name: 'CC' };
      component.ngOnChanges();
      component.costCentre.name = 'CC Updated';
      component.onSave();
      expect(emitSpy).toHaveBeenCalled();
    });
  });

  it('onClose should emit closeEvent', () => {
    const spy = vi.spyOn(component.closeEvent, 'emit');
    component.onClose();
    expect(spy).toHaveBeenCalled();
  });

  it('toggleBudgetDeletion should flip markedForDeletion', () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const budget: any = { markedForDeletion: false };
    component.toggleBudgetDeletion(budget);
    expect(budget.markedForDeletion).toBe(true);
    component.toggleBudgetDeletion(budget);
    expect(budget.markedForDeletion).toBe(false);
  });

  it('removeNewBudget should remove the budget at the given index', () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component.newBudgets = [{ id: null, name: 'A' } as any, { id: null, name: 'B' } as any];
    component.removeNewBudget(0);
    expect(component.newBudgets).toHaveLength(1);
    expect(component.newBudgets[0].name).toBe('B');
  });

  describe('isTeamActive', () => {
    it('should return true for an active team', () => {
      component.teams = [{ id: 1, isActive: true } as unknown as TeamDto];
      expect(component.isTeamActive(1)).toBe(true);
    });

    it('should return false for an inactive team', () => {
      component.teams = [{ id: 2, isActive: false } as unknown as TeamDto];
      expect(component.isTeamActive(2)).toBe(false);
    });

    it('should return true when team is not found (treat as active)', () => {
      component.teams = [];
      expect(component.isTeamActive(999)).toBe(true);
    });
  });

  it('getBudgetFieldError teamId should return inactive team error for inactive team', () => {
    component.teams = [{ id: 2, isActive: false } as unknown as TeamDto];
    component.newBudgetDraft = { ...component.newBudgetDraft, teamId: 2 };
    expect(component.getBudgetFieldError('teamId')).toBe('Select an active team.');
  });

  it('onBudgetFieldBlur should mark the field as touched', () => {
    component.onBudgetFieldBlur('name');
    expect(component.touchedBudgetFields['name']).toBe(true);
  });
});
