import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
  DuplicatePaymentRequestByUserDto,
  PaymentRequestByUserDto,
} from '../../../../types/exporter';
import { EuroPipe } from '../../../../pipes/euro.pipe';
import { ModalComponent } from '../../../general/modal-component/modal-component';

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
  @Input() sourceInvoice: PaymentRequestByUserDto | null = null;
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

  onDismissDuplicate(duplicate: DuplicatePaymentRequestByUserDto): void {
    this.dismissDuplicate.emit(duplicate);
  }

  onSubmitRegardless(): void {
    this.submitRegardless.emit();
  }

  getDuplicateUserName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return duplicate.paymentRequestByUser.user?.name ?? 'Unknown user';
  }

  getDuplicateTeamName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return duplicate.paymentRequestByUser.team?.name ?? 'Unknown team';
  }

  getMatchedFieldLabel(field: string): string {
    switch (field) {
      case 'invoiceNumber':
        return 'Invoice number';
      case 'similarInvoiceNumber':
        return 'Similar invoice number';
      case 'amount':
        return 'Amount';
      case 'payday':
        return 'Payday';
      case 'user':
        return 'User';
      case 'team':
        return 'Team';
      default:
        return field;
    }
  }
}
