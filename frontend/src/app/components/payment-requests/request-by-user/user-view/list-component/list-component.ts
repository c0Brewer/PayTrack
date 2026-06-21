import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import {
  PaymentRequestByUserDto,
  PayoutType,
  PayoutTypeLabels,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-user-invoice-list-component',
  imports: [DatePipe, EuroPipe],
  templateUrl: './list-component.html',
  styleUrl: './list-component.scss',
})
export class UserInvoiceListComponent {
  @Input() invoices: PaymentRequestByUserDto[] = [];

  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.openDetail.emit(invoice);
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
