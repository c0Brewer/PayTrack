import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { BankStatementService } from '../../../services/bank-statement-service/bank-statement-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  BankStatementEntryDto,
  BankStatementMatchResultDto,
  TransactionDto,
} from '../../../types/exporter';

import { BankStatementImportComponent } from './bank-statement-import-component';

const mockEntry: BankStatementEntryDto = {
  booking: '2026-05-21T00:00:00.000+02:00',
  partnerName: 'ACME Corp',
  partnerAccount: { iban: 'AT611904300234573201', bic: 'BKAUATWW' },
  amount: { value: 120.5, currency: 'EUR' },
  receiverReference: 'INV-2026-042',
  reference: null,
};

const mockTransaction: TransactionDto = {
  id: 7,
  userId: 1,
  teamId: 2,
  amount: 120.5,
  purposeOfPayment: 'ACME Invoice',
  paymentReference: 'INV-2026-042',
  status: 2,
  budgetId: null,
  paidAt: '2026-05-21T00:00:00.000Z',
};

type ResultRow = BankStatementMatchResultDto & {
  skipped: boolean;
  expanded: boolean;
  _entryId: string;
};

const mockMatchResult: ResultRow = {
  entry: mockEntry,
  hasMatch: true,
  matchedTransaction: mockTransaction,
  matchScore: 85,
  skipped: false,
  expanded: false,
  _entryId: 'entry-0',
};

describe('BankStatementImportComponent', () => {
  let component: BankStatementImportComponent;
  let fixture: ComponentFixture<BankStatementImportComponent>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  let bankStatementServiceMock: any;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  let notificationServiceMock: any;

  beforeEach(async () => {
    bankStatementServiceMock = {
      getMatches: vi.fn(),
      applyUpdates: vi.fn(),
    };
    notificationServiceMock = {
      showSuccess: vi.fn(),
      showError: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [BankStatementImportComponent],
      providers: [
        { provide: BankStatementService, useValue: bankStatementServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BankStatementImportComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => vi.clearAllMocks());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ── Initial state ──────────────────────────────────────────────────────────

  it('should start in upload phase', () => {
    expect(component.phase()).toBe('upload');
  });

  it('should start with no selected file and empty entries', () => {
    expect(component.selectedFileName()).toBeNull();
    expect(component.parsedEntries()).toHaveLength(0);
  });

  // ── File handling ──────────────────────────────────────────────────────────

  it('should show error and reject non-json files', () => {
    const mockEvent = {
      target: { files: [new File([''], 'statement.pdf')] },
    } as unknown as Event;

    component.onFileSelected(mockEvent);

    expect(notificationServiceMock.showError).toHaveBeenCalledWith('Please upload a .json file.');
    expect(component.selectedFileName()).toBeNull();
  });

  it('should not call service when parsedEntries is empty', () => {
    component.submitForMatching();
    expect(bankStatementServiceMock.getMatches).not.toHaveBeenCalled();
  });

  // ── Score helpers ──────────────────────────────────────────────────────────

  describe('scoreLabel', () => {
    it('should return High for score >= 7', () => {
      expect(component.scoreLabel(7)).toBe('High');
      expect(component.scoreLabel(11)).toBe('High');
    });

    it('should return Medium for score 5–6', () => {
      expect(component.scoreLabel(5)).toBe('Medium');
      expect(component.scoreLabel(6)).toBe('Medium');
    });

    it('should return Low for score < 5', () => {
      expect(component.scoreLabel(0)).toBe('Low');
      expect(component.scoreLabel(3)).toBe('Low');
    });

    it('should return empty string for undefined', () => {
      expect(component.scoreLabel(undefined)).toBe('');
    });
  });

  describe('scoreColor', () => {
    it('should return high badge class for score >= 7', () => {
      expect(component.scoreColor(7)).toBe('confidence-badge confidence-badge--high');
    });

    it('should return medium badge class for score 5–6', () => {
      expect(component.scoreColor(5)).toBe('confidence-badge confidence-badge--medium');
    });

    it('should return low badge class for score < 5', () => {
      expect(component.scoreColor(3)).toBe('confidence-badge confidence-badge--low');
    });

    it('should return none badge class for undefined', () => {
      expect(component.scoreColor(undefined)).toBe('confidence-badge confidence-badge--none');
    });
  });

  // ── Format helpers ─────────────────────────────────────────────────────────

  describe('formatAmount', () => {
    it('should format amount with currency', () => {
      expect(component.formatAmount(mockEntry)).toBe('120.50 EUR');
    });

    it('should return em-dash for missing entry', () => {
      expect(component.formatAmount(undefined)).toBe('—');
    });

    it('should return em-dash for entry without amount', () => {
      expect(component.formatAmount({ ...mockEntry, amount: undefined })).toBe('—');
    });
  });

  describe('formatDate', () => {
    it('should return em-dash for undefined input', () => {
      expect(component.formatDate(undefined)).toBe('—');
    });

    it('should return a formatted date string (de-AT dd.MM.yyyy)', () => {
      const result = component.formatDate('2026-05-21T00:00:00.000Z');
      expect(result).toMatch(/\d{2}\.\d{2}\.\d{4}/);
    });
  });

  describe('formatIban', () => {
    it('should group IBAN into blocks of 4 with spaces', () => {
      expect(component.formatIban('AT611904300234573201')).toBe('AT61 1904 3002 3457 3201');
    });

    it('should strip existing spaces before grouping', () => {
      expect(component.formatIban('AT61 1904 3002 3457 3201')).toBe('AT61 1904 3002 3457 3201');
    });
  });

  // ── Signal mutations ───────────────────────────────────────────────────────

  describe('toggleSkip', () => {
    beforeEach(() => component.results.set([{ ...mockMatchResult }]));

    it('should set skipped to true', () => {
      component.toggleSkip('entry-0');
      expect(component.results()[0].skipped).toBe(true);
    });

    it('should toggle skipped back to false on second call', () => {
      component.toggleSkip('entry-0');
      component.toggleSkip('entry-0');
      expect(component.results()[0].skipped).toBe(false);
    });

    it('should only affect the targeted entry', () => {
      component.results.set([{ ...mockMatchResult }, { ...mockMatchResult, _entryId: 'entry-1' }]);
      component.toggleSkip('entry-0');
      expect(component.results()[0].skipped).toBe(true);
      expect(component.results()[1].skipped).toBe(false);
    });
  });

  describe('toggleExpand', () => {
    beforeEach(() => component.results.set([{ ...mockMatchResult }]));

    it('should set expanded to true', () => {
      component.toggleExpand('entry-0');
      expect(component.results()[0].expanded).toBe(true);
    });

    it('should toggle expanded back to false on second call', () => {
      component.toggleExpand('entry-0');
      component.toggleExpand('entry-0');
      expect(component.results()[0].expanded).toBe(false);
    });

    it('should not affect other entries', () => {
      component.results.set([{ ...mockMatchResult }, { ...mockMatchResult, _entryId: 'entry-1' }]);
      component.toggleExpand('entry-0');
      expect(component.results()[1].expanded).toBe(false);
    });
  });

  describe('getStatusLabel', () => {
    it('should return Unknown when the matched transaction status is missing', () => {
      expect(component.getStatusLabel(undefined)).toBe('Unknown');
    });
  });

  // ── Computed counts ────────────────────────────────────────────────────────

  describe('computed counts', () => {
    it('should correctly reflect matched, skipped, and unmatched', () => {
      component.results.set([
        { ...mockMatchResult, hasMatch: true, skipped: false },
        { ...mockMatchResult, _entryId: 'entry-1', hasMatch: true, skipped: true },
        { ...mockMatchResult, _entryId: 'entry-2', hasMatch: false, skipped: false },
      ]);

      expect(component.matchedCount()).toBe(1);
      expect(component.skippedCount()).toBe(1);
      expect(component.unmatchedCount()).toBe(1);
    });

    it('should return zero counts for empty results', () => {
      component.results.set([]);
      expect(component.matchedCount()).toBe(0);
      expect(component.skippedCount()).toBe(0);
      expect(component.unmatchedCount()).toBe(0);
    });
  });

  // ── submitForMatching ──────────────────────────────────────────────────────

  describe('submitForMatching', () => {
    beforeEach(() => component.parsedEntries.set([mockEntry]));

    it('should switch to review phase and populate results on success', async () => {
      bankStatementServiceMock.getMatches.mockReturnValue(
        of({
          results: [
            {
              entry: mockEntry,
              hasMatch: true,
              matchedTransaction: mockTransaction,
              matchScore: 85,
            },
          ],
        }),
      );

      component.submitForMatching();
      await fixture.whenStable();

      expect(component.phase()).toBe('review');
      expect(component.results()).toHaveLength(1);
      expect(component.results()[0].expanded).toBe(false);
      expect(component.results()[0].skipped).toBe(false);
      expect(component.isLoading()).toBe(false);
    });

    it('should show error notification and stay on upload phase on failure', async () => {
      bankStatementServiceMock.getMatches.mockReturnValue(
        throwError(() => new Error('Network error')),
      );

      component.submitForMatching();
      await fixture.whenStable();

      expect(notificationServiceMock.showError).toHaveBeenCalledWith('Network error');
      expect(component.phase()).toBe('upload');
      expect(component.isLoading()).toBe(false);
    });
  });

  // ── confirmUpdates ─────────────────────────────────────────────────────────

  describe('confirmUpdates', () => {
    beforeEach(() => {
      component.results.set([{ ...mockMatchResult }]);
      component.phase.set('review');
    });

    it('should reset to upload phase and show success notification', async () => {
      bankStatementServiceMock.applyUpdates.mockReturnValue(of([mockTransaction]));

      component.confirmUpdates();
      component.proceedWithSubmit();
      await fixture.whenStable();

      expect(component.phase()).toBe('upload');
      expect(notificationServiceMock.showSuccess).toHaveBeenCalled();
    });

    it('should pass skipped flag and matched transaction id to service', async () => {
      bankStatementServiceMock.applyUpdates.mockReturnValue(of([]));
      component.results.set([
        { ...mockMatchResult, skipped: true },
        { ...mockMatchResult, _entryId: 'entry-1', hasMatch: false, matchedTransaction: undefined },
      ]);

      component.confirmUpdates();
      component.proceedWithSubmit();
      await fixture.whenStable();

      const payload = bankStatementServiceMock.applyUpdates.mock.calls[0][0];
      expect(payload[0]).toMatchObject({
        entryId: 'entry-0',
        skipped: true,
        matchedTransactionId: 7,
      });
      expect(payload[1]).toMatchObject({
        entryId: 'entry-1',
        skipped: false,
        matchedTransactionId: null,
      });
    });

    it('should show error notification on failure', async () => {
      bankStatementServiceMock.applyUpdates.mockReturnValue(
        throwError(() => new Error('Update failed')),
      );

      component.confirmUpdates();
      component.proceedWithSubmit();
      await fixture.whenStable();

      expect(notificationServiceMock.showError).toHaveBeenCalledWith('Update failed');
      expect(component.isLoading()).toBe(false);
    });
  });

  // ── reset ──────────────────────────────────────────────────────────────────

  describe('reset', () => {
    it('should clear all state and return to upload phase', () => {
      component.phase.set('review');
      component.selectedFileName.set('file.json');
      component.parsedEntries.set([mockEntry]);
      component.results.set([{ ...mockMatchResult }]);

      component.reset();

      expect(component.phase()).toBe('upload');
      expect(component.selectedFileName()).toBeNull();
      expect(component.parsedEntries()).toHaveLength(0);
      expect(component.results()).toHaveLength(0);
      expect(component.isLoading()).toBe(false);
    });
  });
});
