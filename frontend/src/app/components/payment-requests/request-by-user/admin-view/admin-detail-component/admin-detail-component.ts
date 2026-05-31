import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { CostCentreService } from '../../../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  ApprovePaymentRequestByUserDto,
  CostCentreDto,
  DeclinePaymentRequestByUserDto,
  GetPaymentRequestsByUserByIdOptions,
  MarkPaymentRequestByUserAsPaidDto,
  PaymentRequestByUserDto,
  RequestChangesPaymentRequestByUserDto,
  TransactionStatus,
} from '../../../../../types/exporter';
import { ExternalNotificationComponent } from '../../../../general/external-notification-component/external-notification-component';
import {
  ChangeRequestContactMethod,
  InvoiceDetailComponent,
  RequestChangesSubmission,
} from '../../general/detail-component/detail-component';

@Component({
  selector: 'app-request-detail-component',
  imports: [InvoiceDetailComponent, ExternalNotificationComponent],
  templateUrl: './admin-detail-component.html',
  styleUrl: './admin-detail-component.scss',
})
export class RequestDetailComponent implements OnInit, OnDestroy {
  private readonly invoiceIncludes: GetPaymentRequestsByUserByIdOptions = {
    IncludeUser: true,
    IncludeTeam: true,
    IncludeBankAccount: true,
    IncludeStatusHistory: true,
  };

  constructor(
    private readonly service: PaymentRequestByUserService,
    private readonly costCentreService: CostCentreService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  invoice: PaymentRequestByUserDto | null = null;
  receiptBlobUrl: string | null = null;
  rawReceiptBlobUrl: string | null = null;
  receiptMimeType: string = '';
  isReceiptImage: boolean = false;
  loading: boolean = true;
  markingPaid: boolean = false;
  statusActionPending: string | null = null;
  costCentres: CostCentreDto[] = [];
  modalType: 'email' | 'slack' | null = null;
  changeRequestNotificationReason: string | null = null;
  pendingChangeRequest: RequestChangesPaymentRequestByUserDto | null = null;
  undoingStatusChange: boolean = false;
  canUndoLastStatusChange: boolean = false;

  ngOnInit(): void {
    this.costCentreService.getCostCentres({ Limit: 100 }).subscribe({
      next: (data) => {
        this.costCentres = data.items?.filter((costCentre) => costCentre.isActive !== false) ?? [];
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load cost centres: ' + err.message);
      },
    });

    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.loadInvoice(id);

      this.service.downloadReceipt(id).subscribe({
        next: (blob) => {
          if (this.rawReceiptBlobUrl) URL.revokeObjectURL(this.rawReceiptBlobUrl);
          this.rawReceiptBlobUrl = URL.createObjectURL(blob);
          this.receiptMimeType = blob.type;
          this.isReceiptImage = blob.type.startsWith('image/');
          const isDisplayable = this.isReceiptImage || blob.type.startsWith('application/pdf');
          this.receiptBlobUrl = isDisplayable ? this.rawReceiptBlobUrl : null;
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load receipt: ' + err.message);
        },
      });
    });
  }

  ngOnDestroy(): void {
    if (this.rawReceiptBlobUrl) URL.revokeObjectURL(this.rawReceiptBlobUrl);
  }

  onDownloadReceipt(): void {
    if (!this.rawReceiptBlobUrl) return;
    const ext = this.getExtensionFromMimeType(this.receiptMimeType);
    const filename = `${this.invoice?.invoiceNumber ?? 'receipt'}${ext}`;
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = this.rawReceiptBlobUrl;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  onMarkPaid(markPaidRequest: MarkPaymentRequestByUserAsPaidDto): void {
    if (!this.invoice || this.markingPaid) return;

    this.markingPaid = true;
    this.service.markPaymentRequestByUserAsPaid(this.invoice.id, markPaidRequest).subscribe({
      next: () => {
        this.loadInvoice(this.invoice!.id);
        this.markingPaid = false;
        this.canUndoLastStatusChange = true;
        this.notificationService.showSuccess('Invoice marked as paid');
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.markingPaid = false;
        this.notificationService.showError('Could not mark invoice as paid: ' + err.message);
        this.cdr.detectChanges();
      },
    });
  }

  onApprove(approveRequest: ApprovePaymentRequestByUserDto): void {
    this.runStatusAction('approve', 'Invoice approved', 'Could not approve invoice: ', () =>
      this.service.approvePaymentRequestByUser(this.invoice!.id, approveRequest),
    );
  }

  onDecline(declineRequest: DeclinePaymentRequestByUserDto): void {
    this.runStatusAction('decline', 'Invoice declined', 'Could not decline invoice: ', () =>
      this.service.declinePaymentRequestByUser(this.invoice!.id, declineRequest),
    );
  }

  onRequestChanges(requestChangesRequest: RequestChangesSubmission): void {
    const { contactMethod, ...request } = requestChangesRequest;

    if (contactMethod !== 'none') {
      this.openChangeRequestNotification(contactMethod, request);
      return;
    }

    this.runStatusAction('requestChanges', 'Changes requested', 'Could not request changes: ', () =>
      this.service.requestChangesForPaymentRequestByUser(this.invoice!.id, request),
    );
  }

  onNotificationSent(): void {
    if (!this.pendingChangeRequest) return;

    const request = this.pendingChangeRequest;
    this.pendingChangeRequest = null;
    this.modalType = null;
    this.runStatusAction('requestChanges', 'Changes requested', 'Could not request changes: ', () =>
      this.service.requestChangesForPaymentRequestByUser(this.invoice!.id, request),
    );
  }

  onNotificationModalClosed(): void {
    this.modalType = null;
    this.pendingChangeRequest = null;
    this.changeRequestNotificationReason = null;
  }

  onUndoStatusChange(): void {
    if (!this.invoice || this.undoingStatusChange) return;

    this.undoingStatusChange = true;
    this.service.undoLastStatusChange(this.invoice.id).subscribe({
      next: () => {
        this.loadInvoice(this.invoice!.id);
        this.undoingStatusChange = false;
        this.canUndoLastStatusChange = false;
        this.notificationService.showSuccess('Status change undone');
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.undoingStatusChange = false;
        this.notificationService.showError('Could not undo status change: ' + err.message);
        this.cdr.detectChanges();
      },
    });
  }

  get notificationEmail(): string {
    return this.invoice?.user?.email ?? '';
  }

  get notificationSubject(): string {
    return `Changes requested for invoice ${this.invoice?.invoiceNumber ?? ''}`.trim();
  }

  get notificationMessage(): string {
    if (!this.invoice) return '';

    const name = this.invoice.user?.name ?? 'User';
    const invoiceNumber = this.invoice.invoiceNumber;
    const amount = this.invoice.amount.toLocaleString('de-DE', {
      style: 'currency',
      currency: 'EUR',
    });
    const reason = this.changeRequestNotificationReason ?? this.latestChangeRequestReason;

    if (this.modalType === 'email') {
      return (
        `Dear ${name},\n\n` +
        `Changes were requested for invoice ${invoiceNumber} (${amount}).\n` +
        (reason ? `Reason: ${reason}\n` : '') +
        `Please review and update the invoice at your earliest convenience.\n\n` +
        `Best regards,\nPayTrack`
      );
    }

    return (
      `Changes were requested for invoice ${invoiceNumber} (${amount}). ` +
      (reason ? `Reason: ${reason} ` : '') +
      `Please review and update the invoice.`
    );
  }

  private runStatusAction(
    action: string,
    successMessage: string,
    errorPrefix: string,
    request: () => ReturnType<PaymentRequestByUserService['approvePaymentRequestByUser']>,
    afterSuccess?: () => void,
  ): void {
    if (!this.invoice || this.statusActionPending) return;

    this.statusActionPending = action;
    request().subscribe({
      next: () => {
        this.loadInvoice(this.invoice!.id);
        this.statusActionPending = null;
        this.canUndoLastStatusChange = true;
        this.notificationService.showSuccess(successMessage);
        afterSuccess?.();
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.statusActionPending = null;
        this.notificationService.showError(errorPrefix + err.message);
        this.cdr.detectChanges();
      },
    });
  }

  private loadInvoice(id: number): void {
    this.service.getPaymentRequestsByUserById(id, this.invoiceIncludes).subscribe({
      next: (data) => {
        this.invoice = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not load invoice: ' + err.message);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  private getExtensionFromMimeType(mimeType: string): string {
    const map: Record<string, string> = {
      'image/jpeg': '.jpg',
      'image/png': '.png',
      'application/pdf': '.pdf',
    };
    return map[mimeType] ?? '';
  }

  private get latestChangeRequestReason(): string | null {
    const history = this.invoice?.statusHistory ?? [];
    const latestEntry = [...history]
      .reverse()
      .find(
        (entry) => entry.toStatus === TransactionStatus.ChangesRequested && !!entry.comment?.trim(),
      );

    return latestEntry?.comment?.trim() ?? null;
  }

  private openChangeRequestNotification(
    contactMethod: Exclude<ChangeRequestContactMethod, 'none'>,
    request: RequestChangesPaymentRequestByUserDto,
  ): void {
    this.pendingChangeRequest = request;
    this.changeRequestNotificationReason = request.reason?.trim() || null;
    this.modalType = contactMethod;
  }

  onBack(): void {
    this.router.navigate(['/requests']);
  }
}
