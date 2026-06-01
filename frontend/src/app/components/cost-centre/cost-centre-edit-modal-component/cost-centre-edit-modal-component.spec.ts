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

  it('formatBudgetAmount should return dash for null and formatted number otherwise', () => {
    expect(component.formatBudgetAmount(null)).toBe('—');
    expect(component.formatBudgetAmount(1500)).toBe('1.500');
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
});
