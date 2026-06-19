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
    expect(compiled.textContent).toContain('Same invoice number');
    expect(compiled.textContent).toContain('Same amount');
  });

  it('should render source invoice info when provided', () => {
    const duplicate = {
      paymentRequestByUser: {
        id: 2,
        invoiceNumber: 'INV-2',
        amount: 25,
        purposeOfPayment: 'Travel',
        user: { name: 'Max' },
        team: { name: 'Finance' },
      } as PaymentRequestByUserDto,
      score: 150,
      matchedFields: ['similarInvoiceNumber', 'amount', 'team'],
    } as DuplicatePaymentRequestByUserDto;

    fixture.componentRef.setInput('visible', true);
    fixture.componentRef.setInput('sourceInvoice', {
      invoiceNumber: 'INV-1',
      amount: 25,
      purposeOfPayment: 'Travel',
      user: { name: 'Anna' },
      team: { name: 'Finance' },
    });
    fixture.componentRef.setInput('duplicates', [duplicate]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Invoice being checked');
    expect(compiled.textContent).toContain('Matching invoice');
    expect(compiled.textContent).toContain('INV-1');
    expect(compiled.textContent).toContain('Anna');
    expect(compiled.textContent).toContain('INV-2');
    expect(compiled.textContent).toContain('Similar invoice number');
    expect(compiled.textContent).toContain('Same team');
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

  it('should emit source invoice view and delete events after source invoice is selected', () => {
    const invoice = { id: 1, invoiceNumber: 'INV-1' } as PaymentRequestByUserDto;
    const duplicate = {
      paymentRequestByUser: { id: 2, invoiceNumber: 'INV-2' },
      score: 60,
      matchedFields: ['invoiceNumber'],
    } as DuplicatePaymentRequestByUserDto;
    const openSpy = vi.fn();
    const deleteSpy = vi.fn();
    component.openDetail.subscribe(openSpy);
    component.deleteInvoice.subscribe(deleteSpy);
    fixture.componentRef.setInput('sourceInvoice', invoice);
    fixture.detectChanges();

    component.selectInvoice(duplicate, 'source');
    component.onOpenSelectedDetail(duplicate);
    component.onRequestDeleteSelectedInvoice(duplicate);
    component.onConfirmDeleteSelectedInvoice();

    expect(openSpy).toHaveBeenCalledWith(invoice);
    expect(deleteSpy).toHaveBeenCalledWith(invoice);
  });

  it('should emit matching invoice view and delete events after matching invoice is selected', () => {
    const duplicate = {
      paymentRequestByUser: { id: 2, invoiceNumber: 'INV-2' } as PaymentRequestByUserDto,
      score: 60,
      matchedFields: ['invoiceNumber'],
    } as DuplicatePaymentRequestByUserDto;
    const openSpy = vi.fn();
    const deleteSpy = vi.fn();
    component.openDetail.subscribe(openSpy);
    component.deleteInvoice.subscribe(deleteSpy);

    component.selectInvoice(duplicate, 'matching');
    component.onOpenSelectedDetail(duplicate);
    component.onRequestDeleteSelectedInvoice(duplicate);
    expect(component.pendingDeleteInvoice).toBe(duplicate.paymentRequestByUser);

    component.onConfirmDeleteSelectedInvoice();

    expect(openSpy).toHaveBeenCalledWith(duplicate.paymentRequestByUser);
    expect(deleteSpy).toHaveBeenCalledWith(duplicate.paymentRequestByUser);
  });

  it('should not select source invoice when source invoice is only a preview', () => {
    const duplicate = {
      paymentRequestByUser: { id: 2, invoiceNumber: 'INV-2' },
      score: 60,
      matchedFields: ['invoiceNumber'],
    } as DuplicatePaymentRequestByUserDto;
    const openSpy = vi.fn();
    const deleteSpy = vi.fn();
    component.openDetail.subscribe(openSpy);
    component.deleteInvoice.subscribe(deleteSpy);
    fixture.componentRef.setInput('sourceInvoice', { invoiceNumber: 'INV-1' });
    fixture.detectChanges();

    component.selectInvoice(duplicate, 'source');
    component.onOpenSelectedDetail(duplicate);
    component.onRequestDeleteSelectedInvoice(duplicate);

    expect(openSpy).not.toHaveBeenCalled();
    expect(deleteSpy).not.toHaveBeenCalled();
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
