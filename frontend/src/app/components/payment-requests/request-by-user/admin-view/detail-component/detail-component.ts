import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import { DisableOfflineActionDirective } from '../../../../../directives/disable-offline-action.directive';
import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { OfflineService } from '../../../../../services/offline/offline-service';
import {
  ApprovePaymentRequestByUserDto,
  CostCentreDto,
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
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';
import { DetailComponent } from '../../../../general/detail-component/detail-component';
import { ModalComponent } from '../../../../general/modal-component/modal-component';

@Component({
  selector: 'app-admin-invoice-detail-component',
  imports: [
    DatePipe,
    DetailComponent,
    EuroPipe,
    FormsModule,
    BoxComponent,
    DisableOfflineActionDirective,
    ModalComponent,
  ],
  templateUrl: './detail-component.html',
  styleUrl: './detail-component.scss',
})
export class AdminInvoiceDetailComponent implements OnChanges {
  readonly reasonMinLength = 3;
  readonly reasonMaxLength = 1000;
  readonly paymentReferenceMinLength = 3;
  readonly paymentReferenceMaxLength = 255;
  readonly paymentPurposeMinLength = 3;
  readonly paymentPurposeMaxLength = 500;
  protected readonly offlineService = inject(OfflineService);

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
  @Input() costCentres: CostCentreDto[] = [];
  @Output() downloadReceipt = new EventEmitter<void>();
  @Output() approve = new EventEmitter<ApprovePaymentRequestByUserDto>();
  @Output() decline = new EventEmitter<DeclinePaymentRequestByUserDto>();
  @Output() requestChanges = new EventEmitter<RequestChangesPaymentRequestByUserDto>();
  @Output() markPaid = new EventEmitter<MarkPaymentRequestByUserAsPaidDto>();
  @Output() back = new EventEmitter<void>();

  TransactionStatusLabels = TransactionStatusLabels;
  PayoutTypeLabels = PayoutTypeLabels;
  TransactionStatus = TransactionStatus;
  paymentReference: string = '';
  paymentPurpose: string = '';
  paymentDate: string = new Date().toISOString().split('T')[0];
  maxPaymentDate: string = new Date().toISOString().split('T')[0];
  approvalCostCentreId: number | null = null;
  approvalReason: string = '';
  declineReason: string = '';
  changeRequestReason: string = '';
  activeActionModal: 'approve' | 'requestChanges' | 'decline' | 'markPaid' | null = null;
  declineReasonBlurred: boolean = false;
  changeRequestReasonBlurred: boolean = false;
  paymentReferenceBlurred: boolean = false;
  paymentPurposeBlurred: boolean = false;

  constructor(private readonly sanitizer: DomSanitizer) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['invoice'] && this.activeActionModal && !this.isActionModalAllowed(this.activeActionModal)) {
      this.closeActionModal();
    }
  }

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

  openActionModal(action: 'approve' | 'requestChanges' | 'decline' | 'markPaid'): void {
    if (!this.isActionModalAllowed(action)) {
      return;
    }

    this.resetValidationForAction(action);
    this.activeActionModal = action;
  }

  closeActionModal(): void {
    this.activeActionModal = null;
    this.resetValidationForAction('approve');
    this.resetValidationForAction('requestChanges');
    this.resetValidationForAction('decline');
    this.resetValidationForAction('markPaid');
  }

  onApprove(): void {
    if (!this.approvalCostCentreId) {
      return;
    }

    this.approve.emit({
      costCentreId: this.approvalCostCentreId,
      reason: this.approvalReason.trim() || null,
    });
  }

  onDecline(): void {
    if (!this.isTextLengthValid(this.declineReason, this.reasonMinLength, this.reasonMaxLength)) {
      return;
    }

    this.decline.emit({
      reason: this.declineReason.trim(),
    });
  }

  onRequestChanges(): void {
    if (
      !this.isTextLengthValid(this.changeRequestReason, this.reasonMinLength, this.reasonMaxLength)
    ) {
      return;
    }

    this.requestChanges.emit({
      reason: this.changeRequestReason.trim(),
    });
  }

  onMarkPaid(): void {
    if (
      !this.paymentDate ||
      !this.isTextLengthValid(
        this.paymentReference,
        this.paymentReferenceMinLength,
        this.paymentReferenceMaxLength,
      ) ||
      !this.isTextLengthValid(
        this.paymentPurpose,
        this.paymentPurposeMinLength,
        this.paymentPurposeMaxLength,
      )
    ) {
      return;
    }

    this.markPaid.emit({
      paymentReference: this.paymentReference.trim(),
      purposeOfPayment: this.paymentPurpose.trim(),
      paymentDate: new Date(this.paymentDate).toISOString(),
    });
  }

  isTextTooShort(value: string, minLength: number): boolean {
    const trimmedLength = value.trim().length;

    return trimmedLength > 0 && trimmedLength < minLength;
  }

  isTextTooLong(value: string, maxLength: number): boolean {
    return value.trim().length > maxLength;
  }

  isTextLengthValid(value: string, minLength: number, maxLength: number): boolean {
    const trimmedLength = value.trim().length;

    return trimmedLength >= minLength && trimmedLength <= maxLength;
  }

  markFieldBlurred(
    field: 'declineReason' | 'changeRequestReason' | 'paymentReference' | 'paymentPurpose',
  ): void {
    switch (field) {
      case 'declineReason':
        this.declineReasonBlurred = true;
        break;
      case 'changeRequestReason':
        this.changeRequestReasonBlurred = true;
        break;
      case 'paymentReference':
        this.paymentReferenceBlurred = true;
        break;
      case 'paymentPurpose':
        this.paymentPurposeBlurred = true;
        break;
    }
  }

  private isActionModalAllowed(action: 'approve' | 'requestChanges' | 'decline' | 'markPaid'): boolean {
    if (!this.invoice) {
      return false;
    }

    switch (action) {
      case 'approve':
        return this.canManageStatus && this.canApprove(this.invoice.status);
      case 'requestChanges':
        return this.canManageStatus && this.canRequestChanges(this.invoice.status);
      case 'decline':
        return this.canManageStatus && this.canDecline(this.invoice.status);
      case 'markPaid':
        return this.canMarkPaid && this.invoice.status === TransactionStatus.Approved;
    }
  }

  private resetValidationForAction(action: 'approve' | 'requestChanges' | 'decline' | 'markPaid'): void {
    if (action === 'requestChanges') {
      this.changeRequestReasonBlurred = false;
      return;
    }

    if (action === 'decline') {
      this.declineReasonBlurred = false;
      return;
    }

    if (action === 'markPaid') {
      this.paymentReferenceBlurred = false;
      this.paymentPurposeBlurred = false;
    }
  }
}
