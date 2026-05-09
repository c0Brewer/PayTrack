import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import {
  PaymentRequestByUserDto,
  PayoutType,
  PayoutTypeLabels,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
  MarkPaymentRequestByUserAsPaidDto,
} from '../../../types/exporter';

@Component({
  selector: 'app-invoice-detail-component',
  imports: [CurrencyPipe, DatePipe, FormsModule],
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
  @Input() canMarkPaid: boolean = false;
  @Input() markingPaid: boolean = false;
  @Output() downloadReceipt = new EventEmitter<void>();
  @Output() markPaid = new EventEmitter<MarkPaymentRequestByUserAsPaidDto>();
  @Output() back = new EventEmitter<void>();

  TransactionStatusLabels = TransactionStatusLabels;
  PayoutTypeLabels = PayoutTypeLabels;
  TransactionStatus = TransactionStatus;
  paymentReference: string = '';
  paymentPurpose: string = '';
  paymentDate: string = new Date().toISOString().split('T')[0];
  maxPaymentDate: string = new Date().toISOString().split('T')[0];

  constructor(private readonly sanitizer: DomSanitizer) {}

  get safeReceiptUrl(): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.receiptBlobUrl ?? '');
  }

  getStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }

  getPayoutTypeLabel(type: PayoutType): string {
    return PayoutTypeLabels[type] ?? 'Unknown';
  }

  onMarkPaid(): void {
    if (!this.paymentReference.trim() || !this.paymentPurpose.trim() || !this.paymentDate) {
      return;
    }

    this.markPaid.emit({
      paymentReference: this.paymentReference.trim(),
      purposeOfPayment: this.paymentPurpose.trim(),
      paymentDate: new Date(this.paymentDate).toISOString(),
    });
  }
}
