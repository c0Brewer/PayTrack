import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
  PaymentRequestByUserDto,
  PayoutType,
  PayoutTypeLabels,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-invoice-list-component',
  imports: [DatePipe],
  templateUrl: './list-component.html',
  styleUrl: './list-component.scss',
})
export class InvoiceListComponent {
  @Input() invoices: PaymentRequestByUserDto[] = [];
  @Input() showUserNameColumn: boolean = false;
  @Input() showDuplicateIndicator: boolean = false;

  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();
  @Output() openDuplicates = new EventEmitter<PaymentRequestByUserDto>();

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.openDetail.emit(invoice);
  }

  onOpenDuplicates(invoice: PaymentRequestByUserDto): void {
    this.openDuplicates.emit(invoice);
  }

  getPayoutTypeLabel(type: PayoutType): string {
    return PayoutTypeLabels[type] ?? 'Unknown';
  }

  getTransactionStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }
}
