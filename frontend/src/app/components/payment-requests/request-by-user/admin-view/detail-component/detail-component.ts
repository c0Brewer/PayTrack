import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
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

@Component({
  selector: 'app-admin-invoice-detail-component',
  imports: [
    DatePipe,
    DetailComponent,
    EuroPipe,
    FormsModule,
    BoxComponent,
    DisableOfflineActionDirective,
  ],
  templateUrl: './detail-component.html',
  styleUrl: './detail-component.scss',
})
export class AdminInvoiceDetailComponent {
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
  PayoutType = PayoutType;
  paymentReference: string = '';
  paymentPurpose: string = '';
  paymentDate: string = new Date().toISOString().split('T')[0];
  maxPaymentDate: string = new Date().toISOString().split('T')[0];
  approvalCostCentreId: number | null = null;
  approvalReason: string = '';
  declineReason: string = '';
  changeRequestReason: string = '';

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
    if (!this.declineReason.trim()) {
      return;
    }

    this.decline.emit({
      reason: this.declineReason.trim(),
    });
  }

  onRequestChanges(): void {
    if (!this.changeRequestReason.trim()) {
      return;
    }

    this.requestChanges.emit({
      reason: this.changeRequestReason.trim(),
    });
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
