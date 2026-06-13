import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import Papa from 'papaparse';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../services/payment-request-by-team/payment-request-by-team-service';
import { SystemSettingService } from '../../../../services/system-setting/system-setting-service';
import { BudgetDto } from '../../../../types/exporter';

import { CsvBulkImportModalComponent } from './csv-bulk-import-modal';

const MOCK_FILE = new File([''], 'test.csv', { type: 'text/csv' });

const MOCK_TEAMS = [{ id: 1, name: 'Team A' }];
const MOCK_COST_CENTRES = [{ id: 10, name: 'CC Alpha' }];
const MOCK_USERS = [
  { id: 100, primaryText: 'Alice Müller', secondaryText: 'alice@example.com' },
  { id: 101, primaryText: 'Bob Smith', secondaryText: 'bob@example.com' },
];

const TOMORROW = ((): string => {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().slice(0, 10);
})();

describe('CsvBulkImportModalComponent', () => {
  let component: CsvBulkImportModalComponent;
  let fixture: ComponentFixture<CsvBulkImportModalComponent>;

  const mockNotificationService = { showSuccess: vi.fn(), showError: vi.fn() };
  const mockPaymentRequestByTeamService = { createPaymentRequestByTeam: vi.fn() };
  const mockSystemSettingService = { getCsvColumnSettings: vi.fn() };

  beforeEach(async () => {
    mockNotificationService.showSuccess.mockClear();
    mockNotificationService.showError.mockClear();
    mockPaymentRequestByTeamService.createPaymentRequestByTeam.mockReset().mockReturnValue(of({}));
    mockSystemSettingService.getCsvColumnSettings
      .mockReset()
      .mockReturnValue(of({ nameColumn: 'Name', summeColumn: 'Summe' }));

    // Prevent parseCsvFile from running during component creation in most tests.
    // Tests that verify CSV parsing will re-spy individually.
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (vi.spyOn(Papa, 'parse') as any).mockImplementation((_file: any, config: Papa.ParseConfig) => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      config.complete?.({ data: [], errors: [], meta: {} as Papa.ParseMeta }, undefined as any);
      return {} as Papa.Parser;
    });

    await TestBed.configureTestingModule({
      imports: [CsvBulkImportModalComponent, ReactiveFormsModule],
      providers: [
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: PaymentRequestByTeamService, useValue: mockPaymentRequestByTeamService },
        { provide: SystemSettingService, useValue: mockSystemSettingService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CsvBulkImportModalComponent);
    component = fixture.componentInstance;

    component.file = MOCK_FILE;
    component.teams = MOCK_TEAMS;
    component.costCentres = MOCK_COST_CENTRES;
    component.allUsers = MOCK_USERS;
    component.incomeBudgets = [];

    fixture.detectChanges();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  // ─── BASIC ───

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start at configure step', () => {
    expect(component.step).toBe('configure');
  });

  it('should have an invalid configForm when empty', () => {
    expect(component.configForm.invalid).toBe(true);
  });

  // ─── parseEuroAmount ───

  describe('parseEuroAmount', () => {
    it('returns 0 for "0,00 €"', () => {
      expect(component.parseEuroAmount('0,00 €')).toBe(0);
    });

    it('returns 12.5 for "12,50 €"', () => {
      expect(component.parseEuroAmount('12,50 €')).toBe(12.5);
    });

    it('handles thousands separator: "1.234,56 €" → 1234.56', () => {
      expect(component.parseEuroAmount('1.234,56 €')).toBe(1234.56);
    });

    it('returns 0 for empty string', () => {
      expect(component.parseEuroAmount('')).toBe(0);
    });

    it('returns 0 for whitespace-only string', () => {
      expect(component.parseEuroAmount('   ')).toBe(0);
    });

    it('returns 0 for non-numeric string', () => {
      expect(component.parseEuroAmount('N/A €')).toBe(0);
    });
  });

  // ─── CSV PARSING ───

  describe('CSV parsing via parseCsvFile', () => {
    function setupPapaMock(rows: string[][]): void {
      (vi.spyOn(Papa, 'parse') as ReturnType<typeof vi.spyOn>).mockImplementation(
        (_file: unknown, config: Papa.ParseConfig) => {
          config.complete?.({ data: rows, errors: [], meta: {} as Papa.ParseMeta }, undefined);
          return {} as Papa.Parser;
        },
      );
    }

    it('skips pre-header rows and finds the correct data header row', () => {
      setupPapaMock([
        ['', '', 'Produkt 1', '', 'Produkt 2'],
        ['', '', '', '', ''],
        ['Name', 'Status', 'Summe', 'Bezahlt'],
        ['Alice Müller', 'Mitglied', '15,00 €', 'FALSE'],
      ]);
      component.ngOnInit();
      expect(component.parsedRows).toHaveLength(1);
      expect(component.parsedRows[0].rawName).toBe('Alice Müller');
      expect(component.parsedRows[0].amount).toBe(15);
    });

    it('skips rows with empty Name column', () => {
      setupPapaMock([
        ['Name', 'Summe'],
        ['', '10,00 €'],
        ['   ', '5,00 €'],
        ['Bob Smith', '20,00 €'],
      ]);
      component.ngOnInit();
      expect(component.parsedRows).toHaveLength(1);
      expect(component.parsedRows[0].rawName).toBe('Bob Smith');
    });

    it('skips rows with zero amount', () => {
      setupPapaMock([
        ['Name', 'Summe'],
        ['Alice Müller', '0,00 €'],
        ['Bob Smith', '5,00 €'],
      ]);
      component.ngOnInit();
      expect(component.parsedRows).toHaveLength(1);
      expect(component.parsedRows[0].rawName).toBe('Bob Smith');
    });

    it('includes valid rows with positive amounts', () => {
      setupPapaMock([
        ['Name', 'Summe'],
        ['Alice Müller', '10,50 €'],
        ['Bob Smith', '25,00 €'],
      ]);
      component.ngOnInit();
      expect(component.parsedRows).toHaveLength(2);
    });

    it('shows error and emits close when no header row found', () => {
      setupPapaMock([
        ['Foo', 'Bar', 'Baz'],
        ['A', 'B', 'C'],
      ]);
      const closeSpy = vi.spyOn(component.closeEvent, 'emit');
      component.ngOnInit();
      expect(mockNotificationService.showError).toHaveBeenCalled();
      expect(closeSpy).toHaveBeenCalled();
    });
  });

  // ─── USER MATCHING ───

  describe('user matching in buildPreviewRows', () => {
    beforeEach(() => {
      component.parsedRows = [
        { rawName: 'Alice Müller', amount: 10 },
        { rawName: 'Unknown Person', amount: 20 },
      ];
    });

    it('auto-matches users by exact case-insensitive name', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      (component as any).buildPreviewRows();
      const alice = component.previewRows.find((r) => r.rawName === 'Alice Müller')!;
      expect(alice.isAutoMatched).toBe(true);
      expect(alice.userId).toBe(100);
    });

    it('leaves unmatched rows with userId=null', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      (component as any).buildPreviewRows();
      const unknown = component.previewRows.find((r) => r.rawName === 'Unknown Person')!;
      expect(unknown.isAutoMatched).toBe(false);
      expect(unknown.userId).toBeNull();
    });

    it('treats ambiguous names (two users with same name) as unmatched', () => {
      component.allUsers = [
        { id: 1, primaryText: 'Same Name' },
        { id: 2, primaryText: 'Same Name' },
      ];
      component.parsedRows = [{ rawName: 'Same Name', amount: 10 }];
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      (component as any).buildPreviewRows();
      expect(component.previewRows[0].isAutoMatched).toBe(false);
      expect(component.previewRows[0].userId).toBeNull();
    });

    it('populates unmatchedNames with only unmatched raw names', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      (component as any).buildPreviewRows();
      expect(component.unmatchedNames).toEqual(['Unknown Person']);
    });
  });

  // ─── STATE MACHINE ───

  describe('state machine', () => {
    function fillValidForm(): void {
      component.configForm.setValue({
        teamId: 1,
        budgetId: 10,
        purposeOfPayment: 'Test purpose',
        dueDate: TOMORROW,
      });
    }

    it('onNextClicked with invalid form stays on configure step and marks form touched', () => {
      component.onNextClicked();
      expect(component.step).toBe('configure');
      expect(component.configForm.touched).toBe(true);
    });

    it('onNextClicked with valid form and no unmatched rows → step=preview', () => {
      fillValidForm();
      component.parsedRows = [{ rawName: 'Alice Müller', amount: 10 }];
      component.onNextClicked();
      expect(component.step).toBe('preview');
      expect(component.showWarningModal).toBe(false);
    });

    it('onNextClicked with valid form and unmatched rows → shows warning modal', () => {
      fillValidForm();
      component.parsedRows = [{ rawName: 'Nobody Known', amount: 10 }];
      component.onNextClicked();
      expect(component.showWarningModal).toBe(true);
      expect(component.step).toBe('configure');
    });

    it('onWarningOkClicked hides warning modal and advances to preview', () => {
      component.showWarningModal = true;
      component.onWarningOkClicked();
      expect(component.showWarningModal).toBe(false);
      expect(component.step).toBe('preview');
    });

    it('onBackClicked returns to configure step', () => {
      component.step = 'preview';
      component.onBackClicked();
      expect(component.step).toBe('configure');
    });

    it('allRowsAssigned returns false when any row has userId=null', () => {
      component.previewRows = [
        {
          rawName: 'A',
          amount: 1,
          userId: 1,
          displayName: 'A',
          isAutoMatched: true,
          status: 'pending',
        },
        {
          rawName: 'B',
          amount: 2,
          userId: null,
          displayName: null,
          isAutoMatched: false,
          status: 'pending',
        },
      ];
      expect(component.allRowsAssigned).toBe(false);
    });

    it('allRowsAssigned returns true when all rows have a userId', () => {
      component.previewRows = [
        {
          rawName: 'A',
          amount: 1,
          userId: 1,
          displayName: 'A',
          isAutoMatched: true,
          status: 'pending',
        },
        {
          rawName: 'B',
          amount: 2,
          userId: 2,
          displayName: 'B',
          isAutoMatched: true,
          status: 'pending',
        },
      ];
      expect(component.allRowsAssigned).toBe(true);
    });

    it('onUserAssigned sets userId and displayName on the correct row', () => {
      component.previewRows = [
        {
          rawName: 'Unknown',
          amount: 5,
          userId: null,
          displayName: null,
          isAutoMatched: false,
          status: 'pending',
        },
      ];
      component.onUserAssigned(0, { id: 100, primaryText: 'Alice Müller' });
      expect(component.previewRows[0].userId).toBe(100);
      expect(component.previewRows[0].displayName).toBe('Alice Müller');
    });

    it('onClose emits closeEvent', () => {
      const spy = vi.spyOn(component.closeEvent, 'emit');
      component.onClose();
      expect(spy).toHaveBeenCalled();
    });
  });

  // ─── SUBMISSION ───

  describe('submission', () => {
    function setupReadyToSubmit(): void {
      component.configForm.setValue({
        teamId: 1,
        budgetId: 10,
        purposeOfPayment: 'Bulk test',
        dueDate: TOMORROW,
      });
      component.previewRows = [
        {
          rawName: 'Alice Müller',
          amount: 10,
          userId: 100,
          displayName: 'Alice Müller',
          isAutoMatched: true,
          status: 'pending',
        },
        {
          rawName: 'Bob Smith',
          amount: 20,
          userId: 101,
          displayName: 'Bob Smith',
          isAutoMatched: true,
          status: 'pending',
        },
      ];
    }

    it('onSubmitAll does not call service when rows have unassigned users', () => {
      component.previewRows = [
        {
          rawName: 'Unknown',
          amount: 5,
          userId: null,
          displayName: null,
          isAutoMatched: false,
          status: 'pending',
        },
      ];
      component.onSubmitAll();
      expect(mockPaymentRequestByTeamService.createPaymentRequestByTeam).not.toHaveBeenCalled();
    });

    it('calls createPaymentRequestByTeam once per row', () => {
      setupReadyToSubmit();
      component.onSubmitAll();
      expect(mockPaymentRequestByTeamService.createPaymentRequestByTeam).toHaveBeenCalledTimes(2);
    });

    it('all rows succeed → all status=success and step=results', () => {
      setupReadyToSubmit();
      component.onSubmitAll();
      expect(component.previewRows.every((r) => r.status === 'success')).toBe(true);
      expect(component.step).toBe('results');
    });

    it('one row fails → that row status=error with message, others succeed, step=results', () => {
      setupReadyToSubmit();
      mockPaymentRequestByTeamService.createPaymentRequestByTeam
        .mockReturnValueOnce(throwError(() => new Error('Server error')))
        .mockReturnValue(of({}));
      component.onSubmitAll();
      expect(component.previewRows[0].status).toBe('error');
      expect(component.previewRows[0].errorMessage).toBe('Server error');
      expect(component.previewRows[1].status).toBe('success');
      expect(component.step).toBe('results');
    });

    it('successCount and failureCount return correct values', () => {
      component.previewRows = [
        {
          rawName: 'A',
          amount: 1,
          userId: 1,
          displayName: 'A',
          isAutoMatched: true,
          status: 'success',
        },
        {
          rawName: 'B',
          amount: 2,
          userId: 2,
          displayName: 'B',
          isAutoMatched: true,
          status: 'error',
        },
        {
          rawName: 'C',
          amount: 3,
          userId: 3,
          displayName: 'C',
          isAutoMatched: true,
          status: 'success',
        },
      ];
      expect(component.successCount).toBe(2);
      expect(component.failureCount).toBe(1);
    });
  });

  // ─── getCostCentreName ───

  describe('getCostCentreName', () => {
    it('should return name when cost centre is found', () => {
      expect(component.getCostCentreName(10)).toBe('CC Alpha');
    });

    it('should return empty string when cost centre is not found', () => {
      expect(component.getCostCentreName(999)).toBe('');
    });
  });

  // ─── BUDGET LOADING ───

  describe('budget loading', () => {
    it('should filter incomeBudgets by teamId when teamId changes', () => {
      component.incomeBudgets = [
        { id: 1, name: 'Budget A', teamId: 1 },
        { id: 2, name: 'Budget B', teamId: 2 },
        { id: 3, name: 'Budget C', teamId: 1 },
      ] as unknown as BudgetDto[];

      component.configForm.get('teamId')!.setValue(1);
      expect(component.budgets).toHaveLength(2);
      expect(component.budgets.map((b) => b.id)).toEqual([1, 3]);
    });

    it('should clear budgets when teamId changes to null', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      component.budgets = [{ id: 1, name: 'Old' } as any];
      component.configForm.get('teamId')!.setValue(null);
      expect(component.budgets).toEqual([]);
    });

    it('should show empty budgets when no incomeBudgets match the teamId', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      component.incomeBudgets = [{ id: 5, name: 'Other', teamId: 99 } as any];
      component.configForm.get('teamId')!.setValue(1);
      expect(component.budgets).toEqual([]);
    });

    it('buildPayload sets budgetId to undefined when form value is null', () => {
      component.configForm.get('teamId')!.setValue(1, { emitEvent: false });
      component.configForm.get('budgetId')!.setValue(null, { emitEvent: false });
      component.configForm.get('purposeOfPayment')!.setValue('Test', { emitEvent: false });
      component.configForm.get('dueDate')!.setValue('2099-01-01', { emitEvent: false });
      const row = {
        rawName: 'Alice',
        amount: 10,
        userId: 100,
        displayName: 'Alice',
        isAutoMatched: true,
        status: 'pending' as const,
      };
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const payload = (component as any).buildPayload(row);
      expect(payload.transaction.budgetId).toBeUndefined();
    });
  });

  // ─── getError ───

  describe('getError', () => {
    it('should return null for unknown field', () => {
      expect(component.getError('nonExistentField')).toBeNull();
    });

    it('should return null when control is valid and touched', () => {
      component.configForm.get('dueDate')!.setValue(TOMORROW);
      component.configForm.get('dueDate')!.markAsTouched();
      expect(component.getError('dueDate')).toBeNull();
    });

    it('should return null when invalid but not touched', () => {
      expect(component.getError('teamId')).toBeNull();
    });

    it('should return required message', () => {
      component.configForm.get('teamId')!.markAsTouched();
      expect(component.getError('teamId')).toBe('This field is required.');
    });

    it('should return min message when min error is present', () => {
      component.configForm.get('teamId')!.setErrors({ min: { min: 1 } });
      component.configForm.get('teamId')!.markAsTouched();
      expect(component.getError('teamId')).toBe('Minimum value is 1.');
    });

    it('should return maxlength message', () => {
      component.configForm.get('purposeOfPayment')!.setValue('x'.repeat(256));
      component.configForm.get('purposeOfPayment')!.markAsTouched();
      expect(component.getError('purposeOfPayment')).toBe('Maximum length is 255 characters.');
    });

    it('should return minDate message for past due date', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      component.configForm.get('dueDate')!.setValue(yesterday.toISOString().slice(0, 10));
      component.configForm.get('dueDate')!.markAsTouched();
      expect(component.getError('dueDate')).toBe('Due date must be today or in the future.');
    });

    it('should return fallback message for unknown error', () => {
      component.configForm.get('teamId')!.setErrors({ unknownError: true });
      component.configForm.get('teamId')!.markAsTouched();
      expect(component.getError('teamId')).toBe('Invalid value.');
    });
  });

  // ─── isInvalid ───

  describe('isInvalid', () => {
    it('should return false for unknown field', () => {
      expect(component.isInvalid('nonExistentField')).toBe(false);
    });

    it('should return false when invalid but not touched', () => {
      expect(component.isInvalid('teamId')).toBe(false);
    });

    it('should return true when invalid and touched', () => {
      component.configForm.get('teamId')!.markAsTouched();
      expect(component.isInvalid('teamId')).toBe(true);
    });

    it('should return false when valid and touched', () => {
      component.configForm.get('dueDate')!.setValue(TOMORROW);
      component.configForm.get('dueDate')!.markAsTouched();
      expect(component.isInvalid('dueDate')).toBe(false);
    });
  });

  // ─── DESTROY ───

  it('ngOnDestroy should not throw', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
