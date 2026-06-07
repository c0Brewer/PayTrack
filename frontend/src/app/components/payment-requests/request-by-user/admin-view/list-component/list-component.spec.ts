import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentRequestByUserDto } from '../../../../../types/exporter';

import { AdminInvoiceListComponent } from './list-component';

describe('AdminInvoiceListComponent', () => {
  let component: AdminInvoiceListComponent;
  let fixture: ComponentFixture<AdminInvoiceListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminInvoiceListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminInvoiceListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show empty state when no invoices', () => {
    component.invoices = [];
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No invoices found');
  });

  it('should emit invoice when onOpenDetail is called', () => {
    const invoice = { id: 1, amount: 100 } as PaymentRequestByUserDto;
    let emitted: PaymentRequestByUserDto | undefined;
    component.openDetail.subscribe((inv) => (emitted = inv));

    component.onOpenDetail(invoice);

    expect(emitted).toEqual(invoice);
  });

  it('should emit invoice when onOpenDuplicates is called', () => {
    const invoice = { id: 1, amount: 100 } as PaymentRequestByUserDto;
    let emitted: PaymentRequestByUserDto | undefined;
    component.openDuplicates.subscribe((inv) => (emitted = inv));

    component.onOpenDuplicates(invoice);

    expect(emitted).toEqual(invoice);
  });

  it('should render duplicate badge for potential duplicates', () => {
    fixture.componentRef.setInput('invoices', [
      {
        id: 1,
        amount: 100,
        invoiceNumber: 'INV-1',
        hasPotentialDuplicate: true,
      } as PaymentRequestByUserDto,
    ]);

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.duplicate-badge')?.textContent).toContain('Duplicates');
  });

  it('should return correct text for getPayoutTypeLabel', () => {
    expect(component.getPayoutTypeLabel(0)).toBe('Pay to User');
    expect(component.getPayoutTypeLabel(1)).toBe('Pay to Supplier');
  });

  it('should return correct text for getTransactionStatusLabel', () => {
    expect(component.getTransactionStatusLabel(0)).toBe('Submitted');
    expect(component.getTransactionStatusLabel(1)).toBe('Changes Requested');
    expect(component.getTransactionStatusLabel(2)).toBe('Approved');
    expect(component.getTransactionStatusLabel(3)).toBe('Paid');
    expect(component.getTransactionStatusLabel(4)).toBe('Declined');
  });
});
