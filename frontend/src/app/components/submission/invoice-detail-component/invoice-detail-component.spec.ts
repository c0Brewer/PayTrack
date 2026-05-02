import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentRequestByUserDto, PayoutType, TransactionStatus } from '../../../types/exporter';

import { InvoiceDetailComponent } from './invoice-detail-component';

describe('InvoiceDetailComponent', () => {
  let component: InvoiceDetailComponent;
  let fixture: ComponentFixture<InvoiceDetailComponent>;

  const mockInvoice = {
    id: 1,
    invoiceNumber: 'INV-001',
    status: TransactionStatus.Submitted,
    amount: 99.99,
    team: { name: 'Engineering' },
    purposeOfPayment: 'Conference ticket',
    payoutType: PayoutType.External,
    comment: 'Annual conference',
    createdAt: '2026-01-01T00:00:00Z',
    paidAt: null,
    bankAccount: { iban: 'AT611904300234573201' },
    user: { name: 'Alice' },
    statusHistory: [],
  } as unknown as PaymentRequestByUserDto;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoiceDetailComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InvoiceDetailComponent);
    component = fixture.componentInstance;
  });

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
    expect(fixture.nativeElement.querySelector('.download-btn')).not.toBeNull();
  });

  it('should not show download button when hasReceipt is false', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = false;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.download-btn')).toBeNull();
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

  it('should emit back event when back button is clicked', () => {
    fixture.detectChanges();
    let emitted = false;
    component.back.subscribe(() => (emitted = true));
    (fixture.nativeElement.querySelector('.back-btn') as HTMLButtonElement).click();
    expect(emitted).toBe(true);
  });

  it('should emit downloadReceipt when download button is clicked', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.hasReceipt = true;
    fixture.detectChanges();
    let emitted = false;
    component.downloadReceipt.subscribe(() => (emitted = true));
    (fixture.nativeElement.querySelector('.download-btn') as HTMLButtonElement).click();
    expect(emitted).toBe(true);
  });

  it('should return correct status labels', () => {
    expect(component.getStatusLabel(TransactionStatus.Submitted)).toBe('Submitted');
    expect(component.getStatusLabel(TransactionStatus.ChangesRequested)).toBe('Changes requested');
    expect(component.getStatusLabel(TransactionStatus.Approved)).toBe('Approved');
    expect(component.getStatusLabel(TransactionStatus.Paid)).toBe('Paid');
    expect(component.getStatusLabel(TransactionStatus.Declined)).toBe('Declined');
    expect(component.getStatusLabel(99 as TransactionStatus)).toBe('Unknown');
  });

  it('should return correct payout type labels', () => {
    expect(component.getPayoutTypeLabel(PayoutType.User)).toBe('User');
    expect(component.getPayoutTypeLabel(PayoutType.External)).toBe('External');
    expect(component.getPayoutTypeLabel(99 as PayoutType)).toBe('Unknown');
  });

  it('should show cost centre when showCostCentre is true and costCentre is set', () => {
    component.invoice = {
      ...mockInvoice,
      costCentre: { id: 1, name: 'CC-Marketing' },
    } as unknown as PaymentRequestByUserDto;
    component.loading = false;
    component.showCostCentre = true;
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Cost Centre');
    expect(text).toContain('CC-Marketing');
  });

  it('should hide cost centre row when showCostCentre is false', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    component.showCostCentre = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Cost Centre');
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
          changedAt: '2026-01-02T00:00:00Z',
          comment: 'Looks good',
        },
      ],
    } as unknown as PaymentRequestByUserDto;
    component.loading = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Status History');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Looks good');
  });

  it('should not render status history table when history is empty', () => {
    component.invoice = mockInvoice;
    component.loading = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Status History');
  });
});
