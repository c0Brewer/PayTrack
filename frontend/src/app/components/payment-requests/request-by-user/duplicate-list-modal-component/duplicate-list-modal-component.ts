import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { EuroPipe } from '../../../../pipes/euro.pipe';
import {
  DuplicatePaymentRequestByUserDto,
  PaymentRequestByUserDto,
} from '../../../../types/exporter';
import { ModalComponent } from '../../../general/modal-component/modal-component';

export type DuplicateInvoiceSummary = {
  id?: number | null;
  invoiceNumber?: string | null;
  amount?: number | null;
  paidAt?: string | null;
  purposeOfPayment?: string | null;
  user?: { name?: string | null } | null;
  team?: { name?: string | null } | null;
};

@Component({
  selector: 'app-duplicate-list-modal-component',
  imports: [DatePipe, EuroPipe, ModalComponent],
  templateUrl: './duplicate-list-modal-component.html',
  styleUrl: './duplicate-list-modal-component.scss',
})
export class DuplicateListModalComponent {
  @Input() visible = false;
  @Input() loading = false;
  @Input() mode: 'review' | 'submit' = 'review';
  @Input() actionInvoiceId: number | null = null;
  @Input() sourceInvoice: DuplicateInvoiceSummary | null = null;
  @Input() duplicates: DuplicatePaymentRequestByUserDto[] = [];

  @Output() closeModal = new EventEmitter<void>();
  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();
  @Output() deleteInvoice = new EventEmitter<PaymentRequestByUserDto>();
  @Output() dismissDuplicate = new EventEmitter<DuplicatePaymentRequestByUserDto>();
  @Output() submitRegardless = new EventEmitter<void>();

  onClose(): void {
    this.closeModal.emit();
  }

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.openDetail.emit(invoice);
  }

  onDeleteInvoice(invoice: PaymentRequestByUserDto): void {
    this.deleteInvoice.emit(invoice);
  }

  onOpenSourceDetail(): void {
    const invoice = this.getActionableSourceInvoice();

    if (invoice) {
      this.openDetail.emit(invoice);
    }
  }

  onDeleteSourceInvoice(): void {
    const invoice = this.getActionableSourceInvoice();

    if (invoice) {
      this.deleteInvoice.emit(invoice);
    }
  }

  onDismissDuplicate(duplicate: DuplicatePaymentRequestByUserDto): void {
    this.dismissDuplicate.emit(duplicate);
  }

  onSubmitRegardless(): void {
    this.submitRegardless.emit();
  }

  getInvoiceUserName(invoice: DuplicateInvoiceSummary | null): string {
    return invoice?.user?.name ?? 'Unknown user';
  }

  getInvoiceTeamName(invoice: DuplicateInvoiceSummary | null): string {
    return invoice?.team?.name ?? 'Unknown team';
  }

  getDuplicateUserName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return this.getInvoiceUserName(duplicate.paymentRequestByUser);
  }

  getDuplicateTeamName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return this.getInvoiceTeamName(duplicate.paymentRequestByUser);
  }

  getActionableSourceInvoice(): PaymentRequestByUserDto | null {
    return this.sourceInvoice?.id ? (this.sourceInvoice as PaymentRequestByUserDto) : null;
  }

  isDuplicateActionPending(duplicate: DuplicatePaymentRequestByUserDto): boolean {
    const sourceInvoiceId = this.getActionableSourceInvoice()?.id;
    const matchingInvoiceId = duplicate.paymentRequestByUser.id;

    return (
      this.actionInvoiceId !== null &&
      (this.actionInvoiceId === sourceInvoiceId || this.actionInvoiceId === matchingInvoiceId)
    );
  }

  getMatchedFieldLabel(field: string): string {
    switch (field) {
      case 'invoiceNumber':
        return 'Same invoice number';
      case 'similarInvoiceNumber':
        return 'Similar invoice number';
      case 'amount':
        return 'Same amount';
      case 'payday':
        return 'Same payday';
      case 'user':
        return 'Same user';
      case 'team':
        return 'Same team';
      default:
        return field;
    }
  }
}
