//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../../services/auth/auth-service';
import { BankAccountService } from '../../../../services/bank-account/bank-account-service';
import { NotificationService } from '../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../services/payment-request-by-user/payment-request-by-user-service';
import { SystemSettingService } from '../../../../services/system-setting/system-setting-service';
import { TeamService } from '../../../../services/team/team-service';
import {
  CreatePaymentRequestByUserDto,
  DuplicatePaymentRequestByUserDto,
  PaymentRequestByUserDto,
  PayoutType,
  TransactionStatus,
} from '../../../../types/exporter';

import { ReceiptSubmitComponent } from './submission-component';

describe('ReceiptSubmitComponent', () => {
  let component: ReceiptSubmitComponent;

  const paymentServiceMock = {
    createPaymentRequestByUser: vi.fn(),
    resubmitPaymentRequestByUser: vi.fn(),
    getDuplicatePaymentRequestsByUser: vi.fn(),
    extractReceiptData: vi.fn(),
  };

  const systemSettingServiceMock = {
    getPublicInvoiceSubmissionSettings: vi.fn(),
  };

  const teamServiceMock = {
    getTeams: vi.fn(),
  };

  const bankAccountServiceMock = {
    getBankAccounts: vi.fn(),
  };

  const notificationMock = {
    showSuccess: vi.fn(),
    showError: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  const activatedRouteMock = {
    snapshot: {
      paramMap: {
        get: vi.fn(() => null),
      },
    },
  };

  const authServiceMock = {
    currentUser$: of(null),
  };

  const setValidFormValues = (): void => {
    component.form.setValue({
      invoiceNumber: 'INV-1',
      comment: '',
      payoutType: PayoutType.User,
      bankAccountId: 1,
      teamId: 1,
      amount: 100,
      purposeOfPayment: 'test',
      paidAt: '2025-01-01',
      receipt: 'ok.pdf',
      creditorName: '',
      dueDate: null,
    });
  };

  beforeEach(async () => {
    paymentServiceMock.createPaymentRequestByUser.mockReset();
    paymentServiceMock.resubmitPaymentRequestByUser.mockReset();
    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReset();
    paymentServiceMock.extractReceiptData.mockReset();
    systemSettingServiceMock.getPublicInvoiceSubmissionSettings
      .mockReset()
      .mockReturnValue(of({ receiptExtractionEnabled: true }));
    teamServiceMock.getTeams.mockReset();
    bankAccountServiceMock.getBankAccounts.mockReset();
    notificationMock.showSuccess.mockReset();
    notificationMock.showError.mockReset();
    routerMock.navigate.mockReset();

    teamServiceMock.getTeams.mockReturnValue(of({ items: [] }));
    bankAccountServiceMock.getBankAccounts.mockReturnValue(of([]));
    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReturnValue(of([]));
    paymentServiceMock.extractReceiptData.mockReturnValue(
      of({
        extractionSucceeded: false,
        message: 'No reliable invoice details were detected.',
        amount: { value: null, confidence: 0 },
        invoiceDate: { value: null, confidence: 0 },
        invoiceNumber: { value: null, confidence: 0 },
      }),
    );

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, ReceiptSubmitComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: SystemSettingService, useValue: systemSettingServiceMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: BankAccountService, useValue: bankAccountServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ReceiptSubmitComponent);
    component = fixture.componentInstance;
  });

  // -------------------------
  // BASIC
  // -------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form on ngOnInit', () => {
    component.ngOnInit();

    expect(component.form).toBeDefined();
    expect(component.form.get('invoiceNumber')).toBeTruthy();
  });

  it('should load teams and bank accounts on init', () => {
    const teams = [{ id: 1, name: 'Team A' }];
    const bankAccounts = [{ id: 1, iban: 'AT611904300234573201' }];
    teamServiceMock.getTeams.mockReturnValue(of({ items: teams }));
    bankAccountServiceMock.getBankAccounts.mockReturnValue(of(bankAccounts));

    component.ngOnInit();

    expect(component.teams).toEqual(teams);
    expect(component.bankAccounts).toEqual(bankAccounts);
  });

  it('should show errors when teams or bank accounts fail to load', () => {
    teamServiceMock.getTeams.mockReturnValue(throwError(() => new Error('teams failed')));
    bankAccountServiceMock.getBankAccounts.mockReturnValue(
      throwError(() => new Error('bank accounts failed')),
    );

    component.ngOnInit();

    expect(notificationMock.showError).toHaveBeenCalledTimes(2);
    expect(notificationMock.showError).toHaveBeenCalledWith('Failed to load teams.');
  });

  // -------------------------
  // FILE VALIDATION
  // -------------------------
  it('should reject invalid file type', () => {
    component.ngOnInit();

    const file = new File(['x'], 'test.txt', { type: 'text/plain' });

    const event = {
      target: { files: [file] },
    } as unknown as Event;

    component.onFileSelected(event);

    expect(component.form.get('receipt')?.errors).toEqual({
      invalidType: true,
    });
  });

  it('should reject oversized file', () => {
    component.ngOnInit();

    const file = new File([new ArrayBuffer(21 * 1024 * 1024)], 'big.pdf', {
      type: 'application/pdf',
    });

    const event = {
      target: { files: [file] },
    } as unknown as Event;

    component.onFileSelected(event);

    expect(component.form.get('receipt')?.errors).toEqual({
      tooLarge: true,
    });
  });

  it('should accept valid file', () => {
    component.ngOnInit();

    const file = new File(['ok'], 'ok.pdf', {
      type: 'application/pdf',
    });

    const event = {
      target: { files: [file] },
    } as unknown as Event;

    component.onFileSelected(event);

    expect(component.selectedFile).toBe(file);
    expect(component.form.get('receipt')?.value).toBe('ok.pdf');
    expect(component.form.get('receipt')?.errors).toBeNull();
    expect(paymentServiceMock.extractReceiptData).toHaveBeenCalledWith(file);
  });

  it('should prefill empty fields from receipt extraction suggestions', () => {
    component.ngOnInit();
    paymentServiceMock.extractReceiptData.mockReturnValue(
      of({
        extractionSucceeded: true,
        message: null,
        amount: { value: 128.5, confidence: 0.82 },
        invoiceDate: { value: '2026-06-10T00:00:00Z', confidence: 0.75 },
        invoiceNumber: { value: 'RE-2026-004812', confidence: 0.78 },
      }),
    );
    const file = new File(['ok'], 'ok.pdf', { type: 'application/pdf' });

    component.onFileSelected({ target: { files: [file] } } as unknown as Event);

    expect(component.form.get('amount')?.value).toBe(128.5);
    expect(component.form.get('paidAt')?.value).toBe('2026-06-10');
    expect(component.form.get('invoiceNumber')?.value).toBe('RE-2026-004812');
    expect(component.form.get('creditorName')?.value).toBeNull();
    expect(component.receiptExtractionStatus).toBe('success');
    expect(component.receiptExtractionMessage).toContain('Pre-filled 3 fields');
  });

  it('should not overwrite manually filled fields with receipt extraction suggestions', () => {
    component.ngOnInit();
    component.form.patchValue({
      amount: 99,
      paidAt: '2026-01-01',
      invoiceNumber: 'MANUAL-1',
      creditorName: 'Manual Supplier',
    });
    paymentServiceMock.extractReceiptData.mockReturnValue(
      of({
        extractionSucceeded: true,
        message: null,
        amount: { value: 128.5, confidence: 0.82 },
        invoiceDate: { value: '2026-06-10', confidence: 0.75 },
        invoiceNumber: { value: 'RE-2026-004812', confidence: 0.78 },
      }),
    );

    component.onFileSelected({
      target: { files: [new File(['ok'], 'ok.pdf', { type: 'application/pdf' })] },
    } as unknown as Event);

    expect(component.form.get('amount')?.value).toBe(99);
    expect(component.form.get('paidAt')?.value).toBe('2026-01-01');
    expect(component.form.get('invoiceNumber')?.value).toBe('MANUAL-1');
    expect(component.form.get('creditorName')?.value).toBe('Manual Supplier');
    expect(component.receiptExtractionStatus).toBe('partial');
    expect(component.receiptExtractionMessage).toBe(
      'Invoice details were detected, but your existing input was kept.',
    );
  });

  it('should show no reliable details only when receipt extraction found no values', () => {
    component.ngOnInit();
    paymentServiceMock.extractReceiptData.mockReturnValue(
      of({
        extractionSucceeded: false,
        message: null,
        amount: { value: null, confidence: 0 },
        invoiceDate: { value: null, confidence: 0 },
        invoiceNumber: { value: null, confidence: 0 },
      }),
    );

    component.onFileSelected({
      target: { files: [new File(['ok'], 'ok.pdf', { type: 'application/pdf' })] },
    } as unknown as Event);

    expect(component.receiptExtractionStatus).toBe('partial');
    expect(component.receiptExtractionMessage).toBe('No reliable invoice details were detected.');
  });

  it('should report only the number of fields actually prefilled', () => {
    component.ngOnInit();
    paymentServiceMock.extractReceiptData.mockReturnValue(
      of({
        extractionSucceeded: true,
        message: null,
        amount: { value: 1200, confidence: 0.95 },
        invoiceDate: { value: null, confidence: 0 },
        invoiceNumber: { value: 'VC-2026-0617', confidence: 0.95 },
      }),
    );

    component.onFileSelected({
      target: { files: [new File(['ok'], 'invoice.jpeg', { type: 'image/jpeg' })] },
    } as unknown as Event);

    expect(component.form.get('amount')?.value).toBe(1200);
    expect(component.form.get('paidAt')?.value).toBe('');
    expect(component.form.get('invoiceNumber')?.value).toBe('VC-2026-0617');
    expect(component.receiptExtractionMessage).toContain('Pre-filled 2 fields');
  });

  it('should keep the form usable when receipt extraction fails', () => {
    component.ngOnInit();
    paymentServiceMock.extractReceiptData.mockReturnValue(
      throwError(() => new Error('Extraction failed')),
    );

    component.onFileSelected({
      target: { files: [new File(['ok'], 'ok.pdf', { type: 'application/pdf' })] },
    } as unknown as Event);

    expect(component.selectedFileName).toBe('ok.pdf');
    expect(component.form.get('receipt')?.errors).toBeNull();
    expect(component.receiptExtractionStatus).toBe('error');
    expect(component.receiptExtractionMessage).toBe('Extraction failed');
  });

  it('should skip receipt extraction while offline', () => {
    component.ngOnInit();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (component as any).offlineService.isOffline.set(true);

    component.onFileSelected({
      target: { files: [new File(['ok'], 'ok.pdf', { type: 'application/pdf' })] },
    } as unknown as Event);

    expect(paymentServiceMock.extractReceiptData).not.toHaveBeenCalled();
    expect(component.receiptExtractionStatus).toBe('partial');
    expect(component.receiptExtractionMessage).toBe(
      'Automatic field detection is unavailable while you are offline.',
    );
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (component as any).offlineService.isOffline.set(false);
  });

  it('should skip receipt extraction when receipt extraction is disabled', () => {
    systemSettingServiceMock.getPublicInvoiceSubmissionSettings.mockReturnValue(
      of({ receiptExtractionEnabled: false }),
    );
    component.ngOnInit();

    component.onFileSelected({
      target: { files: [new File(['ok'], 'ok.pdf', { type: 'application/pdf' })] },
    } as unknown as Event);

    expect(component.selectedFileName).toBe('ok.pdf');
    expect(paymentServiceMock.extractReceiptData).not.toHaveBeenCalled();
    expect(component.receiptExtractionStatus).toBe('idle');
    expect(component.receiptExtractionMessage).toBe('');
  });

  it('should ignore file selection when no file is present', () => {
    component.ngOnInit();

    const event = {
      target: { files: [] },
    } as unknown as Event;

    component.onFileSelected(event);

    expect(component.selectedFile).toBeNull();
    expect(component.form.get('receipt')?.value).toBeNull();
  });

  it('should accept a dropped valid file and ignore empty drops', () => {
    component.ngOnInit();
    const preventDefault = vi.fn();
    const file = new File(['ok'], 'drop.png', { type: 'image/png' });

    component.onFileDropped({
      preventDefault,
      dataTransfer: { files: [] },
    } as unknown as DragEvent);

    expect(preventDefault).toHaveBeenCalledOnce();
    expect(component.selectedFile).toBeNull();

    component.onFileDropped({
      preventDefault: vi.fn(),
      dataTransfer: { files: [file] },
    } as unknown as DragEvent);

    expect(component.selectedFile).toBe(file);
    expect(component.selectedFileName).toBe('drop.png');
  });

  it('should prevent default drag over behavior', () => {
    const preventDefault = vi.fn();

    component.onFileDragOver({ preventDefault } as unknown as DragEvent);

    expect(preventDefault).toHaveBeenCalledOnce();
  });

  it('should shorten IBAN values for display', () => {
    expect(component.getShortenedIban('')).toBe('');
    expect(component.getShortenedIban('AT123456')).toBe('AT123456');
    expect(component.getShortenedIban('AT61 1904 3002 3457 3201')).toBe('AT61 **** **** 3201');
  });

  // -------------------------
  // PAYOUT TYPE
  // -------------------------
  it('should convert payout type correctly', () => {
    expect(component.toPayoutType(PayoutType.User)).toBe(PayoutType.User);
    expect(component.toPayoutType(999)).toBeNull();
  });

  it('should update bank account validation when payout type changes', () => {
    component.ngOnInit();
    const bankAccountControl = component.form.get('bankAccountId')!;
    bankAccountControl.setValue(12);

    component.form.get('payoutType')?.setValue(PayoutType.NotYetPaid);

    expect(bankAccountControl.value).toBe(12);
    expect(bankAccountControl.errors).toBeNull();

    component.form.get('payoutType')?.setValue(PayoutType.User);
    bankAccountControl.markAsTouched();

    expect(bankAccountControl.errors).toBeNull();
  });

  it('should keep payout-specific values when switching payout types', () => {
    component.ngOnInit();

    component.form.get('bankAccountId')?.setValue(12);
    component.form.get('creditorName')?.setValue('Acme GmbH');
    component.form.get('dueDate')?.setValue('2026-06-01');

    component.form.get('payoutType')?.setValue(PayoutType.NotYetPaid);
    component.form.get('payoutType')?.setValue(PayoutType.User);
    component.form.get('payoutType')?.setValue(PayoutType.NotYetPaid);

    expect(component.form.get('bankAccountId')?.value).toBe(12);
    expect(component.form.get('creditorName')?.value).toBe('Acme GmbH');
    expect(component.form.get('dueDate')?.value).toBe('2026-06-01');
  });

  it('should handle string payout type values from radio controls', () => {
    component.ngOnInit();

    component.form.get('payoutType')?.setValue(String(PayoutType.NotYetPaid));

    expect(component.isPayoutType(PayoutType.NotYetPaid)).toBe(true);
    expect(component.form.get('creditorName')?.hasValidator(Validators.required)).toBe(true);
  });

  it('should return validation messages for touched invalid controls', () => {
    component.ngOnInit();
    const amountControl = component.form.get('amount')!;
    amountControl.setValue(0);
    amountControl.markAsTouched();

    const invoiceControl = component.form.get('invoiceNumber')!;
    invoiceControl.setValue('x'.repeat(101));
    invoiceControl.markAsTouched();

    expect(component.getError('missing')).toBeNull();
    expect(component.getError('amount')).toBe('Minimum value is 0.01.');
    expect(component.getError('invoiceNumber')).toBe('Maximum length is 100 characters.');
  });

  it('should report invalid state only for touched invalid controls', () => {
    component.ngOnInit();
    const invoiceControl = component.form.get('invoiceNumber')!;

    expect(component.isInvalid('invoiceNumber')).toBe(false);

    invoiceControl.markAsTouched();

    expect(component.isInvalid('invoiceNumber')).toBe(true);
    expect(component.isInvalid('missing')).toBe(false);
  });

  it('should navigate to bank account management', () => {
    const event = {
      preventDefault: vi.fn(),
    } as unknown as Event;

    component.onManageBankAccountClick(event);

    expect(event.preventDefault).toHaveBeenCalledOnce();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/settings'], { fragment: 'bank-accounts' });
  });

  it('should return display helpers for pending drafts and duplicate metadata', () => {
    component.teams = [{ id: 1, name: 'Powertrain' }];
    const duplicate = {
      paymentRequestByUser: {
        user: { name: 'Alex' },
        team: { name: 'Electronics' },
      },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any;

    expect(component.getPendingStatusLabel('pending')).toBe('Stored Offline');
    expect(component.getPendingStatusClass('pending')).toContain('offline-queue__badge--pending');
    expect(component.getTeamName(1)).toBe('Powertrain');
    expect(component.getTeamName(99)).toBe('Team #99');
    expect(component.getDuplicateUserName(duplicate)).toBe('Alex');
    expect(component.getDuplicateTeamName(duplicate)).toBe('Electronics');
    expect(
      component.getDuplicateUserName({
        paymentRequestByUser: {},
      } as DuplicatePaymentRequestByUserDto),
    ).toBe('Unknown user');
    expect(
      component.getDuplicateTeamName({
        paymentRequestByUser: {},
      } as DuplicatePaymentRequestByUserDto),
    ).toBe('Unknown team');
  });

  // -------------------------
  // DUPLICATE CHECK
  // -------------------------
  it('should open duplicate modal and not submit directly when duplicates exist', () => {
    component.ngOnInit();
    const file = new File(['ok'], 'ok.pdf');
    setValidFormValues();
    component.selectedFile = file;

    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReturnValue(
      of([
        {
          paymentRequestByUser: {
            id: 7,
            amount: 100,
            invoiceNumber: 'INV-1',
            user: { id: 11, name: 'Alex' },
            team: { id: 21, name: 'Core Team' },
          },
          score: 150,
          matchedFields: ['invoiceNumber', 'amount', 'payday', 'user', 'team'],
        },
      ]),
    );
    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(of({}));

    component.onSubmit();

    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).toHaveBeenCalledOnce();
    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).toHaveBeenCalledWith({
      TeamId: 1,
      Amount: 100,
      PaidAt: '2025-01-01',
      InvoiceNumber: 'INV-1',
    });
    expect(paymentServiceMock.createPaymentRequestByUser).not.toHaveBeenCalled();
    expect(component.isDuplicateModalOpen).toBe(true);
    expect(component.pendingSubmissionPayload).not.toBeNull();
    expect(component.pendingSubmissionFile).toBe(file);
    expect(component.duplicateCandidates).toHaveLength(1);
    expect(component.isSubmitting).toBe(false);
  });

  it('should submit when user confirms duplicate modal', () => {
    component.ngOnInit();
    const file = new File(['ok'], 'ok.pdf');
    const payload = {
      invoiceNumber: 'INV-1',
      comment: '',
      payoutType: PayoutType.User,
      bankAccountId: 1,
      receipt: '',
      transaction: {
        teamId: 1,
        amount: 100,
        purposeOfPayment: 'test',
        paidAt: '2025-01-01',
      },
    };

    component.pendingSubmissionPayload = payload;
    component.pendingSubmissionFile = file;
    component.isDuplicateModalOpen = true;
    component.duplicateCandidates = [
      {
        paymentRequestByUser: {
          id: 7,
          amount: 100,
          invoiceNumber: 'INV-1',
          user: { id: 11, name: 'Alex' },
          team: { id: 21, name: 'Core Team' },
        },
        score: 150,
        matchedFields: ['invoiceNumber', 'amount', 'payday', 'user', 'team'],
      },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    ] as any;

    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(of({}));

    component.onDuplicateModalSubmitRegardless();

    expect(paymentServiceMock.createPaymentRequestByUser).toHaveBeenCalledWith(payload, file);
    expect(component.isDuplicateModalOpen).toBe(false);
    expect(component.pendingSubmissionPayload).toBeNull();
    expect(component.pendingSubmissionFile).toBeNull();
  });

  it('should clear duplicate modal state on cancel', () => {
    component.isDuplicateModalOpen = true;
    component.pendingSubmissionPayload = {} as CreatePaymentRequestByUserDto;
    component.pendingSubmissionFile = new File(['ok'], 'ok.pdf');
    component.duplicateCandidates = [
      {
        paymentRequestByUser: { id: 1 },
        score: 1,
        matchedFields: [],
      },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    ] as any;

    component.onDuplicateModalCancel();

    expect(component.isDuplicateModalOpen).toBe(false);
    expect(component.duplicateCandidates).toEqual([]);
    expect(component.pendingSubmissionPayload).toBeNull();
    expect(component.pendingSubmissionFile).toBeNull();
  });

  it('should handle duplicate check error', () => {
    component.ngOnInit();

    const file = new File(['ok'], 'ok.pdf');
    setValidFormValues();
    component.selectedFile = file;

    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReturnValue(
      throwError(() => new Error('Duplicate check failed')),
    );

    component.onSubmit();

    expect(notificationMock.showError).toHaveBeenCalledWith('Duplicate check failed');
    expect(component.isSubmitting).toBe(false);
    expect(component.isDuplicateModalOpen).toBe(false);
  });

  // -------------------------
  // SUBMIT SUCCESS
  // -------------------------
  it('should submit successfully', () => {
    component.ngOnInit();

    const file = new File(['ok'], 'ok.pdf');
    setValidFormValues();
    component.selectedFile = file;

    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(of({}));

    component.onSubmit();

    expect(paymentServiceMock.createPaymentRequestByUser).toHaveBeenCalled();
    expect(notificationMock.showSuccess).toHaveBeenCalledWith('Invoice submitted successfully.');
    expect(component.isSubmitting).toBe(false);
  });

  it('should submit external payouts without a bank account id', () => {
    component.ngOnInit();

    const file = new File(['ok'], 'ok.pdf');
    setValidFormValues();
    component.form.get('payoutType')?.setValue(PayoutType.NotYetPaid);
    component.form.get('creditorName')?.setValue('Acme GmbH');
    component.form.get('dueDate')?.setValue('2025-12-31');
    component.selectedFile = file;

    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(of({}));

    component.onSubmit();

    expect(paymentServiceMock.createPaymentRequestByUser).toHaveBeenCalledWith(
      expect.objectContaining({
        payoutType: PayoutType.NotYetPaid,
        bankAccountId: null,
      }),
      file,
    );
  });

  it('should resubmit a change request without requiring a comment', () => {
    component.ngOnInit();
    setValidFormValues();
    component.form.get('comment')?.setValue('');
    component.isEditMode = true;
    component.editingInvoiceId = 7;
    component.selectedFile = null;
    paymentServiceMock.resubmitPaymentRequestByUser.mockReturnValue(of({}));

    component.onSubmit();

    expect(paymentServiceMock.resubmitPaymentRequestByUser).toHaveBeenCalledWith(
      7,
      expect.objectContaining({
        comment: null,
      }),
      null,
    );
  });

  it('should keep not-yet-paid details and existing receipt when editing a change request', () => {
    component.ngOnInit();

    (
      component as unknown as {
        patchInvoice: (invoice: PaymentRequestByUserDto) => void;
      }
    ).patchInvoice({
      id: 7,
      invoiceNumber: 'INV-7',
      comment: null,
      payoutType: PayoutType.NotYetPaid,
      bankAccount: null,
      team: { id: 2, name: 'Team A' },
      amount: 42,
      purposeOfPayment: 'Travel',
      paidAt: '2026-05-01T00:00:00Z',
      creditorName: 'Test Company',
      dueDate: '2026-06-01T00:00:00Z',
      budget: { id: 3, name: 'Marketing' },
      status: TransactionStatus.ChangesRequested,
      statusHistory: [],
    } as unknown as PaymentRequestByUserDto);

    expect(component.form.get('creditorName')?.value).toBe('Test Company');
    expect(component.form.get('dueDate')?.value).toBe('2026-06-01');
    expect(component.form.get('receipt')?.value).toBe('existing-receipt');
    expect(component.form.get('receipt')?.errors).toBeNull();
    expect(component.selectedFileName).toBe('Current receipt will be kept');
  });

  // -------------------------
  // SUBMIT ERROR
  // -------------------------
  it('should handle submit error', () => {
    component.ngOnInit();

    const file = new File(['ok'], 'ok.pdf');
    setValidFormValues();
    component.selectedFile = file;

    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(
      throwError(() => new Error('Upload failed')),
    );

    component.onSubmit();

    expect(notificationMock.showError).toHaveBeenCalledWith('Upload failed');
    expect(component.isSubmitting).toBe(false);
  });

  // -------------------------
  // INVALID SUBMIT (no file)
  // -------------------------
  it('should not submit if form invalid or no file', () => {
    component.ngOnInit();

    paymentServiceMock.createPaymentRequestByUser.mockClear();

    component.form.reset();
    component.selectedFile = null;

    component.onSubmit();

    expect(paymentServiceMock.createPaymentRequestByUser).not.toHaveBeenCalled();
  });
});
