import { ComponentFixture, TestBed } from '@angular/core/testing';

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
});
