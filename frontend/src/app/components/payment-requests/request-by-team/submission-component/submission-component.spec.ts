import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

import { BudgetService } from '../../../../services/budget/budget-service';
import { CostCentreService } from '../../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../services/payment-request-by-team/payment-request-by-team-service';
import { TeamService } from '../../../../services/team/team-service';
import { UserService } from '../../../../services/user/user-service';
import { BudgetDto } from '../../../../types/exporter';

import { PaymentRequestByTeamComponent } from './submission-component';

describe('PaymentRequestByTeamComponent', () => {
  let component: PaymentRequestByTeamComponent;
  let fixture: ComponentFixture<PaymentRequestByTeamComponent>;

  const mockTeamService = { getTeams: vi.fn() };
  const mockBudgetService = { getBudgets: vi.fn() };
  const mockCostCentreService = { getCostCentres: vi.fn() };
  const mockUserService = { getUser: vi.fn() };
  const mockNotificationService = { showSuccess: vi.fn(), showError: vi.fn() };
  const mockPaymentRequestByTeamService = { createPaymentRequestByTeam: vi.fn() };

  beforeEach(async () => {
    mockTeamService.getTeams.mockReset().mockReturnValue(of({ items: [], totalCount: 0 }));
    mockBudgetService.getBudgets.mockReset().mockReturnValue(of({ items: [], totalCount: 0 }));
    mockCostCentreService.getCostCentres
      .mockReset()
      .mockReturnValue(of({ items: [], totalCount: 0 }));
    mockUserService.getUser.mockReset().mockReturnValue(of({ items: [], totalCount: 0 }));
    mockNotificationService.showSuccess.mockClear();
    mockNotificationService.showError.mockClear();
    mockPaymentRequestByTeamService.createPaymentRequestByTeam.mockReset().mockReturnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [PaymentRequestByTeamComponent, ReactiveFormsModule],
      providers: [
        { provide: TeamService, useValue: mockTeamService },
        { provide: BudgetService, useValue: mockBudgetService },
        { provide: CostCentreService, useValue: mockCostCentreService },
        { provide: UserService, useValue: mockUserService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: PaymentRequestByTeamService, useValue: mockPaymentRequestByTeamService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentRequestByTeamComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // -------------------------
  // BASIC
  // -------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have an invalid form when empty', () => {
    expect(component.form.invalid).toBe(true);
  });

  // -------------------------
  // LOAD DATA (teams + income budgets via forkJoin)
  // -------------------------
  it('should populate teams that have matching income budgets on load', () => {
    const past = new Date();
    past.setDate(past.getDate() - 1);
    const future = new Date();
    future.setDate(future.getDate() + 1);
    mockTeamService.getTeams.mockReturnValue(
      of({
        items: [
          { id: 1, name: 'Team A' },
          { id: 2, name: 'Team B' },
        ],
        totalCount: 2,
      }),
    );
    mockBudgetService.getBudgets.mockReturnValue(
      of({
        items: [
          {
            id: 10,
            name: 'Budget',
            teamId: 1,
            periodStart: past.toISOString(),
            periodEnd: future.toISOString(),
          },
        ],
        totalCount: 1,
      }),
    );
    component.ngOnInit();
    expect(component.teams).toHaveLength(1);
    expect(component.teams[0].name).toBe('Team A');
    expect(component.allIncomeBudgets).toHaveLength(1);
  });

  it('should show no teams when no income budgets exist', () => {
    mockTeamService.getTeams.mockReturnValue(
      of({ items: [{ id: 1, name: 'Team A' }], totalCount: 1 }),
    );
    mockBudgetService.getBudgets.mockReturnValue(of({ items: [], totalCount: 0 }));
    component.ngOnInit();
    expect(component.teams).toEqual([]);
  });

  it('should show error when loadData fails', () => {
    mockTeamService.getTeams.mockReturnValue(throwError(() => new Error()));
    component.ngOnInit();
    expect(mockNotificationService.showError).toHaveBeenCalledWith(
      'Failed to load teams and budgets.',
    );
  });

  // -------------------------
  // LOAD COST CENTRES
  // -------------------------
  it('should populate cost centres on load', () => {
    mockCostCentreService.getCostCentres.mockReturnValue(
      of({ items: [{ id: 1, name: 'CC A' }], totalCount: 1 }),
    );
    component.ngOnInit();
    expect(component.costCentres).toHaveLength(1);
  });

  it('should not overwrite cost centres when items is null', () => {
    mockCostCentreService.getCostCentres.mockReturnValue(of({ items: null, totalCount: 0 }));
    component.ngOnInit();
    expect(component.costCentres).toEqual([]);
  });

  it('should show error when cost centres fail to load', () => {
    mockCostCentreService.getCostCentres.mockReturnValue(throwError(() => new Error()));
    component.ngOnInit();
    expect(mockNotificationService.showError).toHaveBeenCalledWith('Failed to load cost centres.');
  });

  // -------------------------
  // LOAD USERS
  // -------------------------
  it('should populate allUsers on load', () => {
    mockUserService.getUser.mockReturnValue(
      of({ items: [{ id: 1, name: 'Alice', email: 'alice@example.com' }], totalCount: 1 }),
    );
    component.ngOnInit();
    expect(component.allUsers).toEqual([
      { id: 1, primaryText: 'Alice', secondaryText: 'alice@example.com' },
    ]);
  });

  it('should use empty string and undefined for null user name/email', () => {
    mockUserService.getUser.mockReturnValue(
      of({ items: [{ id: 2, name: null, email: null }], totalCount: 1 }),
    );
    component.ngOnInit();
    expect(component.allUsers[0].primaryText).toBe('');
    expect(component.allUsers[0].secondaryText).toBeUndefined();
  });

  it('should handle null items in user response', () => {
    mockUserService.getUser.mockReturnValue(of({ items: null, totalCount: 0 }));
    component.ngOnInit();
    expect(component.allUsers).toEqual([]);
  });

  it('should show error when users fail to load', () => {
    mockUserService.getUser.mockReturnValue(throwError(() => new Error()));
    component.ngOnInit();
    expect(mockNotificationService.showError).toHaveBeenCalledWith('Failed to load users.');
  });

  // -------------------------
  // DUE DATE VALIDATION
  // -------------------------
  it('should reject a past due date', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    const isoDate = yesterday.toISOString().slice(0, 10);
    component.form.get('dueDate')!.setValue(isoDate);
    component.form.get('dueDate')!.markAsTouched();
    expect(component.form.get('dueDate')!.invalid).toBe(true);
  });

  it('should accept a future due date', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const isoDate = tomorrow.toISOString().slice(0, 10);
    component.form.get('dueDate')!.setValue(isoDate);
    expect(component.form.get('dueDate')!.valid).toBe(true);
  });

  // -------------------------
  // getError
  // -------------------------
  it('getError should return null for unknown field', () => {
    expect(component.getError('nonExistentField')).toBeNull();
  });

  it('getError should return null when control is valid and touched', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    component.form.get('dueDate')!.setValue(tomorrow.toISOString().slice(0, 10));
    component.form.get('dueDate')!.markAsTouched();
    expect(component.getError('dueDate')).toBeNull();
  });

  it('getError should return null when invalid but not touched', () => {
    expect(component.getError('amount')).toBeNull();
  });

  it('getError should return required message', () => {
    component.form.get('amount')!.markAsTouched();
    expect(component.getError('amount')).toBe('This field is required.');
  });

  it('getError should return min message', () => {
    component.form.get('amount')!.setValue(0);
    component.form.get('amount')!.markAsTouched();
    expect(component.getError('amount')).toBe('Minimum value is 0.01.');
  });

  it('getError should return maxlength message', () => {
    component.form.get('purposeOfPayment')!.setValue('x'.repeat(256));
    component.form.get('purposeOfPayment')!.markAsTouched();
    expect(component.getError('purposeOfPayment')).toBe('Maximum length is 255 characters.');
  });

  it('getError should return minDate message for past due date', () => {
    const yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    component.form.get('dueDate')!.setValue(yesterday.toISOString().slice(0, 10));
    component.form.get('dueDate')!.markAsTouched();
    expect(component.getError('dueDate')).toBe('Due date must be today or in the future.');
  });

  it('getError should return fallback message for unknown error', () => {
    component.form.get('amount')!.setErrors({ unknownError: true });
    component.form.get('amount')!.markAsTouched();
    expect(component.getError('amount')).toBe('Invalid value.');
  });

  // -------------------------
  // isInvalid
  // -------------------------
  it('isInvalid should return false when invalid but not touched', () => {
    expect(component.isInvalid('amount')).toBe(false);
  });

  it('isInvalid should return true when invalid and touched', () => {
    component.form.get('amount')!.markAsTouched();
    expect(component.isInvalid('amount')).toBe(true);
  });

  it('isInvalid should return false when valid and touched', () => {
    component.form.get('amount')!.setValue(10);
    component.form.get('amount')!.markAsTouched();
    expect(component.isInvalid('amount')).toBe(false);
  });

  it('isInvalid should return false for unknown field', () => {
    expect(component.isInvalid('nonExistentField')).toBe(false);
  });

  // -------------------------
  // USER SELECTION
  // -------------------------
  it('onUserSelected should populate userId', () => {
    component.onUserSelected({ id: 1, primaryText: 'Alice', secondaryText: 'alice@example.com' });
    expect(component.form.get('userId')!.value).toBe(1);
  });

  it('onUserCleared should reset userId to null', () => {
    component.onUserSelected({ id: 1, primaryText: 'Alice', secondaryText: 'alice@example.com' });
    component.onUserCleared();
    expect(component.form.get('userId')!.value).toBeNull();
  });

  // -------------------------
  // SUBMIT
  // -------------------------
  it('onSubmit should not call service when form is invalid', () => {
    component.onSubmit();
    expect(mockPaymentRequestByTeamService.createPaymentRequestByTeam).not.toHaveBeenCalled();
  });

  it('onSubmit should submit successfully and reset', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    component.form.setValue({
      userId: 1,
      amount: 50,
      purposeOfPayment: 'Test payment',
      teamId: 1,
      budgetId: 1,
      dueDate: tomorrow.toISOString().slice(0, 10),
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const typeaheadSpy = vi.spyOn((component as any).typeaheadRef, 'reset');
    component.onSubmit();

    expect(mockPaymentRequestByTeamService.createPaymentRequestByTeam).toHaveBeenCalled();
    expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Payment request created.');
    expect(typeaheadSpy).toHaveBeenCalled();
    expect(component.isSubmitting).toBe(false);
  });

  it('onSubmit should handle submit error', () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    component.form.setValue({
      userId: 1,
      amount: 50,
      purposeOfPayment: 'Test payment',
      teamId: 1,
      budgetId: 1,
      dueDate: tomorrow.toISOString().slice(0, 10),
    });

    mockPaymentRequestByTeamService.createPaymentRequestByTeam.mockReturnValue(
      throwError(() => new Error('Server error')),
    );
    component.onSubmit();

    expect(mockNotificationService.showError).toHaveBeenCalledWith('Server error');
    expect(component.isSubmitting).toBe(false);
  });

  // -------------------------
  // BUDGET FILTERING (local, from allIncomeBudgets)
  // -------------------------
  describe('budget filtering on teamId change', () => {
    it('should filter allIncomeBudgets by teamId', () => {
      component.allIncomeBudgets = [
        { id: 1, name: 'Budget A', teamId: 1 },
        { id: 2, name: 'Budget B', teamId: 2 },
        { id: 3, name: 'Budget C', teamId: 1 },
      ] as unknown as BudgetDto[];
      component.form.get('teamId')!.setValue(1);
      expect(component.budgets).toHaveLength(2);
      expect(component.budgets.map((b) => b.id)).toEqual([1, 3]);
    });

    it('should clear budgets when teamId changes to null', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      component.budgets = [{ id: 1, name: 'Old' } as any];
      component.form.get('teamId')!.setValue(null);
      expect(component.budgets).toEqual([]);
    });

    it('should show empty budgets when no allIncomeBudgets match teamId', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      component.allIncomeBudgets = [{ id: 5, name: 'Other', teamId: 99 } as any];
      component.form.get('teamId')!.setValue(1);
      expect(component.budgets).toEqual([]);
    });
  });

  // -------------------------
  // getCostCentreName
  // -------------------------
  describe('getCostCentreName', () => {
    it('should return name when cost centre exists', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      component.costCentres = [{ id: 10, name: 'IT Equipment' } as any];
      expect(component.getCostCentreName(10)).toBe('IT Equipment');
    });

    it('should return empty string when cost centre does not exist', () => {
      component.costCentres = [];
      expect(component.getCostCentreName(999)).toBe('');
    });
  });

  // -------------------------
  // DESTROY
  // -------------------------
  it('ngOnDestroy should not throw', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
