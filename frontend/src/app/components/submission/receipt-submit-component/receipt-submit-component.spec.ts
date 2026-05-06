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

  const setValidFormValues = () => {
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

  // -------------------------
  // PAYOUT TYPE
  // -------------------------
  it('should convert payout type correctly', () => {
    expect(component.toPayoutType(PayoutType.User)).toBe(PayoutType.User);
    expect(component.toPayoutType(999)).toBeNull();
  });

  it('should navigate to bank account management', () => {
    const event = {
      preventDefault: vi.fn(),
    } as unknown as Event;

    component.onManageBankAccountClick(event);

    expect(event.preventDefault).toHaveBeenCalledOnce();
    //TODO: add when actual settings exist expect(routerMock.navigate).toHaveBeenCalledWith(['/bankaccount']);
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
          paymentRequestByUser: { id: 7, amount: 100, invoiceNumber: 'INV-1' },
          score: 3,
          isAmountAndUserMatch: true,
          isAmountAndTeamMatch: true,
          isInvoiceNumberMatch: true,
        },
      ]),
    );
    paymentServiceMock.createPaymentRequestByUser.mockReturnValue(of({}));

    component.onSubmit();

    expect(paymentServiceMock.getDuplicatePaymentRequestsByUser).toHaveBeenCalledOnce();
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
        paymentRequestByUser: { id: 7, amount: 100, invoiceNumber: 'INV-1' },
        score: 3,
        isAmountAndUserMatch: true,
        isAmountAndTeamMatch: true,
        isInvoiceNumberMatch: true,
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
