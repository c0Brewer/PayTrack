import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import {
  PaymentRequestByUserDto,
  PayoutType,
  PayoutTypeLabels,
  TransactionStatus,
  TransactionStatusLabels,
} from '../../../types/exporter';

@Component({
  selector: 'app-invoice-detail-component',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './invoice-detail-component.html',
  styleUrl: './invoice-detail-component.scss',
})
export class InvoiceDetailComponent {
  @Input() invoice: PaymentRequestByUserDto | null = null;
  @Input() showCostCentre: boolean = false;
  @Input() showUserName: boolean = false;
  @Input() receiptBlobUrl: string | null = null;
  @Input() isReceiptImage: boolean = false;
  @Input() hasReceipt: boolean = false;
  @Input() loading: boolean = false;
  @Output() downloadReceipt = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();

  TransactionStatusLabels = TransactionStatusLabels;
  PayoutTypeLabels = PayoutTypeLabels;

  constructor(private readonly sanitizer: DomSanitizer) {}

  get safeReceiptUrl(): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.receiptBlobUrl ?? '');
  }

  getStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getPayoutTypeLabel(type: PayoutType): string {
    return PayoutTypeLabels[type] ?? 'Unknown';
  }
}
