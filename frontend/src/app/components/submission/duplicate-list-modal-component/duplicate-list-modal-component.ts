import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { DuplicatePaymentRequestByUserDto, PaymentRequestByUserDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-duplicate-list-modal-component',
  imports: [CurrencyPipe, DatePipe, ModalComponent],
  templateUrl: './duplicate-list-modal-component.html',
  styleUrl: './duplicate-list-modal-component.scss',
})
export class DuplicateListModalComponent {
  @Input() visible = false;
  @Input() loading = false;
  @Input() sourceInvoice: PaymentRequestByUserDto | null = null;
  @Input() duplicates: DuplicatePaymentRequestByUserDto[] = [];

  @Output() closeModal = new EventEmitter<void>();
  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();

  onClose(): void {
    this.closeModal.emit();
  }

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.openDetail.emit(invoice);
  }

  getDuplicateUserName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return duplicate.paymentRequestByUser.user?.name ?? 'Unknown user';
  }

  getDuplicateTeamName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return duplicate.paymentRequestByUser.team?.name ?? 'Unknown team';
  }
}
