import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import {
  ApprovePaymentRequestByUserDto,
  BudgetDto,
  DeclinePaymentRequestByUserDto,
  MarkPaymentRequestByUserAsPaidDto,
  PaymentRequestByUserDto,
  PayoutType,
  PayoutTypeLabels,
  RequestChangesPaymentRequestByUserDto,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

export type ChangeRequestContactMethod = 'none' | 'email' | 'slack';
export type RequestChangesSubmission = RequestChangesPaymentRequestByUserDto & {
  contactMethod: ChangeRequestContactMethod;
};

@Component({
  selector: 'app-invoice-detail-component',
  imports: [CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './detail-component.html',
  styleUrl: './detail-component.scss',
})
export class InvoiceDetailComponent {
  @Input() invoice: PaymentRequestByUserDto | null = null;
  @Input() showUserName: boolean = false;
  @Input() receiptBlobUrl: string | null = null;
  @Input() isReceiptImage: boolean = false;
  @Input() hasReceipt: boolean = false;
  @Input() loading: boolean = false;
  @Input() canMarkPaid: boolean = false;
  @Input() markingPaid: boolean = false;
  @Input() canManageStatus: boolean = false;
  @Input() statusActionPending: string | null = null;
  @Input() budgets: BudgetDto[] = [];
  @Output() downloadReceipt = new EventEmitter<void>();
  @Output() approve = new EventEmitter<ApprovePaymentRequestByUserDto>();
  @Output() decline = new EventEmitter<DeclinePaymentRequestByUserDto>();
  @Output() requestChanges = new EventEmitter<RequestChangesSubmission>();
  @Output() markPaid = new EventEmitter<MarkPaymentRequestByUserAsPaidDto>();
  @Output() back = new EventEmitter<void>();

  TransactionStatusLabels = TransactionStatusLabels;
  PayoutTypeLabels = PayoutTypeLabels;
  TransactionStatus = TransactionStatus;
  paymentReference: string = '';
  paymentPurpose: string = '';
  paymentDate: string = new Date().toISOString().split('T')[0];
  maxPaymentDate: string = new Date().toISOString().split('T')[0];
  approvalBudgetId: number | null = null;
  approvalReason: string = '';
  declineReason: string = '';
  changeRequestReason: string = '';
  changeRequestContactMethod: ChangeRequestContactMethod = 'none';

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

  canApprove(status: TransactionStatus): boolean {
    return status === TransactionStatus.Submitted || status === TransactionStatus.Review;
  }

  canRequestChanges(status: TransactionStatus): boolean {
    return status === TransactionStatus.Submitted || status === TransactionStatus.Review;
  }

  canDecline(status: TransactionStatus): boolean {
    return status !== TransactionStatus.Paid && status !== TransactionStatus.Declined;
  }

  isReasonValid(reason: string): boolean {
    return reason.trim().length >= 3;
  }

  isReasonTooShort(reason: string): boolean {
    const length = reason.trim().length;
    return length > 0 && length < 3;
  }

  isOptionalReasonValid(reason: string): boolean {
    const length = reason.trim().length;
    return length === 0 || length >= 3;
  }

  onApprove(): void {
    if (!this.approvalBudgetId || !this.isOptionalReasonValid(this.approvalReason)) {
      return;
    }

    this.approve.emit({
      budgetId: this.approvalBudgetId,
      reason: this.approvalReason.trim() || null,
    });
  }

  onDecline(): void {
    if (!this.isReasonValid(this.declineReason)) {
      return;
    }

    this.decline.emit({
      reason: this.declineReason.trim(),
    });
  }

  onRequestChanges(): void {
    if (!this.isReasonValid(this.changeRequestReason)) {
      return;
    }

    this.requestChanges.emit({
      reason: this.changeRequestReason.trim(),
      contactMethod: this.changeRequestContactMethod,
    });
  }

  onMarkPaid(): void {
    if (
      !this.isReasonValid(this.paymentReference) ||
      !this.isReasonValid(this.paymentPurpose) ||
      !this.paymentDate
    ) {
      return;
    }

    this.markPaid.emit({
      paymentReference: this.paymentReference.trim(),
      purposeOfPayment: this.paymentPurpose.trim(),
      paymentDate: new Date(this.paymentDate).toISOString(),
    });
  }
}
