import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentRequestByUserDto } from '../../../types/exporter';

import { InvoiceListComponent } from './invoice-list-component';

describe('InvoiceListComponent', () => {
  let component: InvoiceListComponent;
  let fixture: ComponentFixture<InvoiceListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoiceListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InvoiceListComponent);
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

  it('should return correct text for payoutTypeToText', () => {
    expect(component.payoutTypeToText(0)).toBe('Internal');
    expect(component.payoutTypeToText(1)).toBe('External');
  });

  it('should return correct text for statusToText', () => {
    expect(component.statusToText(0)).toBe('Submitted');
    expect(component.statusToText(1)).toBe('Changes requested');
    expect(component.statusToText(2)).toBe('Approved');
    expect(component.statusToText(3)).toBe('Paid');
    expect(component.statusToText(4)).toBe('Declined');
  });
});
