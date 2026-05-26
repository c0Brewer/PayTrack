//AI helped with the test cases

import { TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { BankAccountService } from '../../../services/bank-account/bank-account-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { TeamService } from '../../../services/team/team-service';
import { PayoutType } from '../../../types/exporter';

import { ReceiptSubmitComponent } from './receipt-submit-component';

describe('ReceiptSubmitComponent', () => {
  let component: ReceiptSubmitComponent;

  const paymentServiceMock = {
    createPaymentRequestByUser: vi.fn(),
    getDuplicatePaymentRequestsByUser: vi.fn(),
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
    });
  };

  beforeEach(async () => {
    paymentServiceMock.createPaymentRequestByUser.mockReset();
    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReset();
    teamServiceMock.getTeams.mockReset();
    bankAccountServiceMock.getBankAccounts.mockReset();
    notificationMock.showSuccess.mockReset();
    notificationMock.showError.mockReset();
    routerMock.navigate.mockReset();

    teamServiceMock.getTeams.mockReturnValue(of({ items: [] }));
    bankAccountServiceMock.getBankAccounts.mockReturnValue(of([]));
    paymentServiceMock.getDuplicatePaymentRequestsByUser.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, ReceiptSubmitComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: TeamService, useValue: teamServiceMock },
        { provide: BankAccountService, useValue: bankAccountServiceMock },
        { provide: NotificationService, useValue: notificationMock },
        { provide: Router, useValue: routerMock },
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

    component.form.get('payoutType')?.setValue(PayoutType.External);

    expect(bankAccountControl.value).toBeNull();
    expect(bankAccountControl.errors).toBeNull();

    component.form.get('payoutType')?.setValue(PayoutType.User);
    bankAccountControl.markAsTouched();

    expect(bankAccountControl.errors).toEqual({ required: true });
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
          score: 2,
          isAmountAndUserMatch: true,
          isInvoiceNumberMatch: true,
          isAmountAndTeamMatch: true,
        },
      ]),
    );
    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(of({}));

    component.onSubmit();

    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).toHaveBeenCalledOnce();
    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).toHaveBeenCalledWith({
      TeamId: 1,
      Amount: 100,
      PaidAt: '2025-01-01T00:00:00.000Z',
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
        paidAt: '2025-01-01T00:00:00.000Z',
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
        score: 2,
        isAmountAndUserMatch: true,
        isInvoiceNumberMatch: true,
        isAmountAndTeamMatch: true,
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
