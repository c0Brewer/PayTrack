import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

import { BudgetService } from '../../../../services/budget/budget-service';
import { CostCentreService } from '../../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../services/payment-request-by-team/payment-request-by-team-service';
import { TeamService } from '../../../../services/team/team-service';
import { UserService } from '../../../../services/user/user-service';

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
  // LOAD TEAMS
  // -------------------------
  it('should populate teams on load', () => {
    mockTeamService.getTeams.mockReturnValue(
      of({ items: [{ id: 1, name: 'Team A' }], totalCount: 1 }),
    );
    component.ngOnInit();
    expect(component.teams).toHaveLength(1);
  });

  it('should not overwrite teams when items is null', () => {
    mockTeamService.getTeams.mockReturnValue(of({ items: null, totalCount: 0 }));
    component.ngOnInit();
    expect(component.teams).toEqual([]);
  });

  it('should show error when teams fail to load', () => {
    mockTeamService.getTeams.mockReturnValue(throwError(() => new Error()));
    component.ngOnInit();
    expect(mockNotificationService.showError).toHaveBeenCalledWith('Failed to load teams.');
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
  // LOAD BUDGETS
  // -------------------------
  describe('loadBudgets', () => {
    it('should load and filter to active named budgets when teamId changes', () => {
      const today = new Date();
      const past = new Date(today);
      past.setDate(past.getDate() - 1);
      const future = new Date(today);
      future.setDate(future.getDate() + 1);
      const farFuture = new Date(today);
      farFuture.setDate(farFuture.getDate() + 7);

      mockBudgetService.getBudgets.mockReturnValue(
        of({
          items: [
            {
              id: 1,
              name: 'Active',
              costCentreId: 1,
              teamId: 1,
              seasonId: 1,
              targetAmount: 100,
              periodStart: past.toISOString(),
              periodEnd: future.toISOString(),
              transactions: [],
            },
            {
              id: 2,
              name: '',
              costCentreId: 1,
              teamId: 1,
              seasonId: 1,
              targetAmount: 100,
              periodStart: past.toISOString(),
              periodEnd: future.toISOString(),
              transactions: [],
            },
            {
              id: 3,
              name: 'Future Only',
              costCentreId: 1,
              teamId: 1,
              seasonId: 1,
              targetAmount: 100,
              periodStart: farFuture.toISOString(),
              periodEnd: farFuture.toISOString(),
              transactions: [],
            },
            {
              id: 4,
              name: 'Expired',
              costCentreId: 1,
              teamId: 1,
              seasonId: 1,
              targetAmount: 100,
              periodStart: past.toISOString(),
              periodEnd: past.toISOString(),
              transactions: [],
            },
          ],
          totalCount: 4,
        }),
      );

      component.form.get('teamId')!.setValue(1);
      expect(mockBudgetService.getBudgets).toHaveBeenCalledWith({ TeamId: 1 });
      expect(component.budgets).toHaveLength(1);
      expect(component.budgets[0].name).toBe('Active');
    });

    it('should handle null items response without error', () => {
      mockBudgetService.getBudgets.mockReturnValue(of({ items: null, totalCount: 0 }));
      component.form.get('teamId')!.setValue(1);
      expect(component.budgets).toEqual([]);
    });

    it('should clear budgets and not call getBudgets when teamId changes to null', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      component.budgets = [{ id: 1, name: 'Old' } as any];
      component.form.get('teamId')!.setValue(null);
      expect(component.budgets).toEqual([]);
      expect(mockBudgetService.getBudgets).not.toHaveBeenCalled();
    });

    it('should show error when budget loading fails', () => {
      mockBudgetService.getBudgets.mockReturnValue(throwError(() => new Error()));
      component.form.get('teamId')!.setValue(1);
      expect(mockNotificationService.showError).toHaveBeenCalledWith('Failed to load budgets.');
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
