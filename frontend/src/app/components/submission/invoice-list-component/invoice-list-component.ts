import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { PaymentRequestByUserDto, PayoutType, PayoutTypeLabels, TransactionStatus, TransactionStatusLabels } from '../../../types/exporter';

@Component({
  selector: 'app-invoice-list-component',
  imports: [DatePipe],
  templateUrl: './invoice-list-component.html',
  styleUrl: './invoice-list-component.scss',
})
export class InvoiceListComponent {
  @Input() invoices: PaymentRequestByUserDto[] = [];
  @Input() showCostCentreColumn: boolean = false;
  @Input() showUserNameColumn: boolean = false;

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
}
