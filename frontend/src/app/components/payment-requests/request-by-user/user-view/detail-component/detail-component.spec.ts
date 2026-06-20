import { ComponentFixture, TestBed } from '@angular/core/testing';

import {
  PaymentRequestByUserDto,
  PayoutType,
  TransactionStatus,
} from '../../../../../types/exporter';

import { UserInvoiceDetailComponent } from './detail-component';

describe('UserInvoiceDetailComponent', () => {
  let component: UserInvoiceDetailComponent;
  let fixture: ComponentFixture<UserInvoiceDetailComponent>;

  const mockInvoice = {
    id: 1,
    invoiceNumber: 'INV-001',
    status: TransactionStatus.Submitted,
    amount: 99.99,
    team: { name: 'Engineering' },
    purposeOfPayment: 'Conference ticket',
    payoutType: PayoutType.NotYetPaid,
    comment: 'Annual conference',
    createdAt: '2026-01-01T00:00:00Z',
    paidAt: null,
    bankAccount: { iban: 'AT611904300234573201' },
    user: { name: 'Alice' },
    statusHistory: [],
  } as unknown as PaymentRequestByUserDto;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserInvoiceDetailComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(UserInvoiceDetailComponent);
    component = fixture.componentInstance;
  });

  function getButtonByText(text: string): HTMLButtonElement | null {
    const buttons = Array.from(
      fixture.nativeElement.querySelectorAll('button'),
    ) as HTMLButtonElement[];

    return buttons.find((button) => (button.textContent ?? '').includes(text)) ?? null;
  }

  function getStatusFormButton(title: string): HTMLButtonElement | null {
    const forms = Array.from(
      fixture.nativeElement.querySelectorAll('.status-form'),
    ) as HTMLFormElement[];
    const form = forms.find((item) => (item.textContent ?? '').includes(title));

    return form?.querySelector('button') ?? null;
  }

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should show loading indicator when loading is true', () => {
    component.loading = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Loading...');
  });

  it('should not show detail card when loading is true', () => {
    component.loading = true;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.detail-card')).toBeNull();
  });

  it('should render invoice fields when invoice is provided', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('INV-001');
    expect(text).toContain('Engineering');
    expect(text).toContain('Conference ticket');
    expect(text).toContain('AT611904300234573201');
  });

  it('should show "Loading receipt..." when hasReceipt is false and receiptBlobUrl is null', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = false;
    component.receiptBlobUrl = null;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Loading receipt...');
  });

  it('should show "Preview not available." when hasReceipt is true but receiptBlobUrl is null', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = true;
    component.receiptBlobUrl = null;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Preview not available.');
  });

  it('should show download button when hasReceipt is true', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = true;
    fixture.detectChanges();
    expect(getButtonByText('Download Receipt')).not.toBeNull();
  });

  it('should not show download button when hasReceipt is false', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = false;
    fixture.detectChanges();
    expect(getButtonByText('Download Receipt')).toBeNull();
  });

  it('should render img tag when isReceiptImage is true and receiptBlobUrl is set', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.receiptBlobUrl = 'blob:test';
    component.isReceiptImage = true;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img.receipt-image')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('iframe.receipt-frame')).toBeNull();
  });

  it('should render iframe when isReceiptImage is false and receiptBlobUrl is set', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.receiptBlobUrl = 'blob:test';
    component.isReceiptImage = false;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('iframe.receipt-frame')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('img.receipt-image')).toBeNull();
  });

  it('should expose a safe receipt url for the current blob', () => {
    component.receiptBlobUrl = 'blob:test';

    expect(component.safeReceiptUrl).toBeTruthy();
  });

  it('should emit back event when back button is clicked', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    fixture.detectChanges();
    let emitted = false;
    component.back.subscribe(() => (emitted = true));
    getButtonByText('Back')?.click();
    expect(emitted).toBe(true);
  });

  it('should emit downloadReceipt when download button is clicked', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = true;
    fixture.detectChanges();
    let emitted = false;
    component.downloadReceipt.subscribe(() => (emitted = true));
    getButtonByText('Download Receipt')?.click();
    expect(emitted).toBe(true);
  });

  it('should return correct status labels', () => {
    expect(component.getStatusLabel(TransactionStatus.Submitted)).toBe('Submitted');
    expect(component.getStatusLabel(TransactionStatus.ChangesRequested)).toBe('Changes Requested');
    expect(component.getStatusLabel(TransactionStatus.Approved)).toBe('Approved');
    expect(component.getStatusLabel(TransactionStatus.Paid)).toBe('Paid');
    expect(component.getStatusLabel(TransactionStatus.Declined)).toBe('Declined');
    expect(component.getStatusLabel(99 as TransactionStatus)).toBe('Unknown');
  });

  it('should return correct payout type labels', () => {
    expect(component.getPayoutTypeLabel(PayoutType.User)).toBe('Pay to User');
    expect(component.getPayoutTypeLabel(PayoutType.NotYetPaid)).toBe('Pay to Supplier');
    expect(component.getPayoutTypeLabel(99 as PayoutType)).toBe('Unknown');
  });

  it('should return correct status action availability', () => {
    expect(component.canApprove(TransactionStatus.Submitted)).toBe(true);
    expect(component.canApprove(TransactionStatus.Review)).toBe(true);
    expect(component.canApprove(TransactionStatus.Approved)).toBe(false);
    expect(component.canRequestChanges(TransactionStatus.Submitted)).toBe(true);
    expect(component.canRequestChanges(TransactionStatus.Review)).toBe(true);
    expect(component.canRequestChanges(TransactionStatus.Paid)).toBe(false);
    expect(component.canDecline(TransactionStatus.Submitted)).toBe(true);
    expect(component.canDecline(TransactionStatus.Paid)).toBe(false);
    expect(component.canDecline(TransactionStatus.Declined)).toBe(false);
  });

  it('should validate trimmed text lengths consistently', () => {
    expect(component.isTextTooShort(' ab ', 3)).toBe(true);
    expect(component.isTextTooShort(' abc ', 3)).toBe(false);
    expect(component.isTextLengthValid(' abc ', 3, 5)).toBe(true);
    expect(component.isTextLengthValid(' ab ', 3, 5)).toBe(false);
  });

  it('should mark fields as blurred for inline validation', () => {
    component.markFieldBlurred('declineReason');
    component.markFieldBlurred('changeRequestReason');
    component.markFieldBlurred('paymentReference');
    component.markFieldBlurred('paymentPurpose');

    expect(component.declineReasonBlurred).toBe(true);
    expect(component.changeRequestReasonBlurred).toBe(true);
    expect(component.paymentReferenceBlurred).toBe(true);
    expect(component.paymentPurposeBlurred).toBe(true);
  });

  it('should emit approve with trimmed optional reason when cost centre is selected', () => {
    const emitted = vi.fn();
    component.approve.subscribe(emitted);
    component.approvalCostCentreId = 12;
    component.approvalReason = ' approved ';

    component.onApprove();

    expect(emitted).toHaveBeenCalledWith({ costCentreId: 12, reason: 'approved' });
  });

  it('should emit approve with null reason when optional reason is blank', () => {
    const emitted = vi.fn();
    component.approve.subscribe(emitted);
    component.approvalCostCentreId = 12;
    component.approvalReason = ' ';

    component.onApprove();

    expect(emitted).toHaveBeenCalledWith({ costCentreId: 12, reason: null });
  });

  it('should not emit approve without cost centre', () => {
    const emitted = vi.fn();
    component.approve.subscribe(emitted);

    component.onApprove();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should emit decline with trimmed reason', () => {
    const emitted = vi.fn();
    component.decline.subscribe(emitted);
    component.declineReason = ' duplicate ';

    component.onDecline();

    expect(emitted).toHaveBeenCalledWith({ reason: 'duplicate' });
  });

  it('should not emit decline without reason', () => {
    const emitted = vi.fn();
    component.decline.subscribe(emitted);
    component.declineReason = ' ';

    component.onDecline();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should not emit decline when reason is shorter than backend minimum', () => {
    const emitted = vi.fn();
    component.decline.subscribe(emitted);
    component.declineReason = 'ab';

    component.onDecline();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should emit request changes with trimmed reason', () => {
    const emitted = vi.fn();
    component.requestChanges.subscribe(emitted);
    component.changeRequestReason = ' upload clearer receipt ';

    component.onRequestChanges();

    expect(emitted).toHaveBeenCalledWith({ reason: 'upload clearer receipt' });
  });

  it('should not emit request changes without reason', () => {
    const emitted = vi.fn();
    component.requestChanges.subscribe(emitted);
    component.changeRequestReason = ' ';

    component.onRequestChanges();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should not emit request changes when reason is shorter than backend minimum', () => {
    const emitted = vi.fn();
    component.requestChanges.subscribe(emitted);
    component.changeRequestReason = 'ab';

    component.onRequestChanges();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should emit mark paid with trimmed values and ISO payment date', () => {
    const emitted = vi.fn();
    component.markPaid.subscribe(emitted);
    component.paymentReference = ' REF-123 ';
    component.paymentPurpose = ' Supplier payout ';
    component.paymentDate = '2026-02-03';

    component.onMarkPaid();

    expect(emitted).toHaveBeenCalledWith({
      paymentReference: 'REF-123',
      purposeOfPayment: 'Supplier payout',
      paymentDate: new Date('2026-02-03').toISOString(),
    });
  });

  it('should not emit mark paid when required fields are missing', () => {
    const emitted = vi.fn();
    component.markPaid.subscribe(emitted);
    component.paymentReference = 'REF-123';
    component.paymentPurpose = '';
    component.paymentDate = '2026-02-03';

    component.onMarkPaid();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should not emit mark paid when payment reference is shorter than backend minimum', () => {
    const emitted = vi.fn();
    component.markPaid.subscribe(emitted);
    component.paymentReference = 'ab';
    component.paymentPurpose = 'Supplier payout';
    component.paymentDate = '2026-02-03';

    component.onMarkPaid();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should not emit mark paid when purpose is shorter than backend minimum', () => {
    const emitted = vi.fn();
    component.markPaid.subscribe(emitted);
    component.paymentReference = 'REF-123';
    component.paymentPurpose = 'ab';
    component.paymentDate = '2026-02-03';

    component.onMarkPaid();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('should render status admin controls when management is enabled for submitted invoice', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.canManageStatus = true;
    component.costCentres = [{ id: 12, name: 'CC-Finance' }];

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.status-actions')).not.toBeNull();
    expect(getStatusFormButton('Approve')).not.toBeNull();
    expect(getStatusFormButton('Request Changes')).not.toBeNull();
    expect(getStatusFormButton('Decline')).not.toBeNull();
  });

  it('should hide status admin controls when management is disabled', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.canManageStatus = false;

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.status-actions')).toBeNull();
  });

  it('should render only decline and mark paid controls for approved invoice', () => {
    component.invoice = {
      ...mockInvoice,
      status: TransactionStatus.Approved,
    } as PaymentRequestByUserDto;
    component.loading = false;
    component.canManageStatus = true;
    component.canMarkPaid = true;

    fixture.detectChanges();

    expect(getStatusFormButton('Approve')).toBeNull();
    expect(getStatusFormButton('Request Changes')).toBeNull();
    expect(getStatusFormButton('Decline')).not.toBeNull();
    expect(getButtonByText('Mark as Paid')).not.toBeNull();
  });

  it('should hide mark paid controls when invoice is not approved', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.canMarkPaid = true;

    fixture.detectChanges();

    expect(getButtonByText('Mark as Paid')).toBeNull();
  });

  it('should disable pending action buttons', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.canManageStatus = true;
    component.approvalCostCentreId = 12;
    component.changeRequestReason = 'missing receipt';
    component.declineReason = 'duplicate';
    component.statusActionPending = 'requestChanges';

    fixture.detectChanges();

    expect(getStatusFormButton('Request Changes')?.disabled).toBe(true);
    expect(getStatusFormButton('Approve')?.disabled).toBe(false);
    expect(getStatusFormButton('Decline')?.disabled).toBe(false);
  });

  it('should show inline validation for too-short decline reason and disable submit', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.canManageStatus = true;
    component.declineReason = 'ab';
    component.markFieldBlurred('declineReason');

    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Reason must be at least 3 characters long.',
    );
    expect(getStatusFormButton('Decline')?.disabled).toBe(true);
  });

  it('should disable mark paid button for too-short payment reference', () => {
    component.invoice = {
      ...mockInvoice,
      status: TransactionStatus.Approved,
    } as PaymentRequestByUserDto;
    component.loading = false;
    component.canMarkPaid = true;
    component.paymentReference = 'ab';
    component.paymentPurpose = 'Supplier payout';
    component.paymentDate = '2026-02-03';

    fixture.detectChanges();

    expect(getButtonByText('Mark as Paid')?.disabled).toBe(true);
  });

  it('should disable mark paid button while marking paid', () => {
    component.invoice = {
      ...mockInvoice,
      status: TransactionStatus.Approved,
    } as PaymentRequestByUserDto;
    component.loading = false;
    component.canMarkPaid = true;
    component.markingPaid = true;
    component.paymentReference = 'REF-123';
    component.paymentPurpose = 'Supplier payout';
    component.paymentDate = '2026-02-03';

    fixture.detectChanges();

    expect(getButtonByText('Saving...')?.disabled).toBe(true);
  });

  it('should show user name row when showUserName is true', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.showUserName = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Alice');
  });

  it('should hide user name row when showUserName is false', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.showUserName = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Alice');
  });

  it('should render status history table when history entries exist', () => {
    component.invoice = {
      ...mockInvoice,
      statusHistory: [
        {
          fromStatus: TransactionStatus.Submitted,
          toStatus: TransactionStatus.Approved,
          changedById: 7,
          changedBy: { name: 'Finance User' },
          changedAt: '2026-01-02T00:00:00Z',
          comment: 'Looks good',
        },
      ],
    } as unknown as PaymentRequestByUserDto;
    component.loading = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Status History');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Looks good');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Finance User');
  });

  it('should not render status history table when history is empty', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Status History');
  });
});
