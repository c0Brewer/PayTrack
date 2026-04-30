import { TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { TeamService } from '../../../services/team/team-service';
import { PayoutType } from '../../../types/exporter';

import { ReceiptSubmitComponent } from './receipt-submit-component';

describe('ReceiptSubmitComponent', () => {
  let component: ReceiptSubmitComponent;

  const paymentServiceMock = {
    createPaymentRequestByUser: vi.fn(),
  };

  const teamServiceMock = {
    getTeams: vi.fn(),
  };

  const notificationMock = {
    showSuccess: vi.fn(),
    showError: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, ReceiptSubmitComponent],
      providers: [
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: TeamService, useValue: teamServiceMock },
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
    teamServiceMock.getTeams.mockReturnValue(of([]));

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
    expect(routerMock.navigate).toHaveBeenCalledWith(['/settings'], { fragment: 'bank-accounts' });
  });

  // -------------------------
  // SUBMIT SUCCESS
  // -------------------------
  it('should submit successfully', () => {
    component.ngOnInit();

    const file = new File(['ok'], 'ok.pdf');

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
