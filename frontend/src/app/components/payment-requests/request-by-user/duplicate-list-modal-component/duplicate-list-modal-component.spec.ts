import { ComponentFixture, TestBed } from '@angular/core/testing';

import {
  DuplicatePaymentRequestByUserDto,
  PaymentRequestByUserDto,
} from '../../../../types/exporter';

import { DuplicateListModalComponent } from './duplicate-list-modal-component';

describe('DuplicateListModalComponent', () => {
  let component: DuplicateListModalComponent;
  let fixture: ComponentFixture<DuplicateListModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DuplicateListModalComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DuplicateListModalComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render duplicate invoices', () => {
    const duplicate = {
      paymentRequestByUser: {
        id: 2,
        invoiceNumber: 'INV-2',
        amount: 25,
        user: { name: 'Max' },
        team: { name: 'Finance' },
      } as PaymentRequestByUserDto,
      score: 150,
      matchedFields: ['invoiceNumber', 'amount', 'team'],
    } as DuplicatePaymentRequestByUserDto;

    fixture.componentRef.setInput('visible', true);
    fixture.componentRef.setInput('duplicates', [duplicate]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('INV-2');
    expect(compiled.textContent).toContain('Finance');
    expect(compiled.textContent).not.toContain('Score:');
    expect(compiled.textContent).toContain('Invoice number');
    expect(compiled.textContent).toContain('Amount');
  });

  it('should emit close event', () => {
    const spy = vi.fn();
    component.closeModal.subscribe(spy);
    component.invoicePendingDelete = { id: 2 } as PaymentRequestByUserDto;

    component.onClose();

    expect(spy).toHaveBeenCalledOnce();
    expect(component.invoicePendingDelete).toBeNull();
  });

  it('should request delete confirmation before emitting delete event', () => {
    const invoice = { id: 2, invoiceNumber: 'INV-2' } as PaymentRequestByUserDto;
    const spy = vi.fn();
    component.deleteInvoice.subscribe(spy);

    component.onDeleteInvoice(invoice);

    expect(component.invoicePendingDelete).toBe(invoice);
    expect(spy).not.toHaveBeenCalled();
  });

  it('should emit delete invoice after confirmation', () => {
    const invoice = { id: 2, invoiceNumber: 'INV-2' } as PaymentRequestByUserDto;
    const spy = vi.fn();
    component.deleteInvoice.subscribe(spy);
    component.invoicePendingDelete = invoice;

    component.onConfirmDeleteInvoice();

    expect(spy).toHaveBeenCalledWith(invoice);
    expect(component.invoicePendingDelete).toBeNull();
  });

  it('should render inline delete confirmation', () => {
    const duplicate = {
      paymentRequestByUser: {
        id: 2,
        invoiceNumber: 'INV-2',
        amount: 25,
      } as PaymentRequestByUserDto,
      score: 60,
      matchedFields: ['invoiceNumber', 'amount', 'team'],
    } as DuplicatePaymentRequestByUserDto;

    fixture.componentRef.setInput('visible', true);
    fixture.componentRef.setInput('duplicates', [duplicate]);
    component.invoicePendingDelete = duplicate.paymentRequestByUser;
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Delete invoice INV-2?');
    expect(compiled.textContent).toContain('This action cannot be undone.');
  });

  it('should render submit mode without admin actions', () => {
    const duplicate = {
      paymentRequestByUser: {
        id: 2,
        invoiceNumber: 'INV-2',
        amount: 25,
      } as PaymentRequestByUserDto,
      score: 60,
      matchedFields: ['invoiceNumber', 'amount', 'team'],
    } as DuplicatePaymentRequestByUserDto;

    fixture.componentRef.setInput('visible', true);
    fixture.componentRef.setInput('mode', 'submit');
    fixture.componentRef.setInput('duplicates', [duplicate]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Submit Regardless');
    expect(compiled.textContent).not.toContain('Score:');
    expect(compiled.textContent).not.toContain('Dismiss warning');
    expect(compiled.textContent).not.toContain('Delete invoice');
  });
});
