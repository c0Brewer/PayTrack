import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  ApprovePaymentRequestByUserDto,
  CostCentreDto,
  DeclinePaymentRequestByUserDto,
  MarkPaymentRequestByUserAsPaidDto,
  PaymentRequestByUserDto,
  RequestChangesPaymentRequestByUserDto,
} from '../../../types/exporter';
import { InvoiceDetailComponent } from '../invoice-detail-component/invoice-detail-component';

@Component({
  selector: 'app-request-detail-component',
  imports: [InvoiceDetailComponent],
  templateUrl: './request-detail-component.html',
  styleUrl: './request-detail-component.scss',
})
export class RequestDetailComponent implements OnInit, OnDestroy {
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

      this.service
        .getPaymentRequestsByUserById(id, {
          IncludeUser: true,
          IncludeTeam: true,
          IncludeCostCentre: true,
          IncludeBankAccount: true,
          IncludeStatusHistory: true,
        })
        .subscribe({
          next: (data) => {
            this.invoice = data;
            this.loading = false;
            this.cdr.detectChanges();
          },
          error: (err: Error) => {
            this.notificationService.showError('Could not load invoice: ' + err.message);
            this.loading = false;
          },
        });

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
      next: (invoice) => {
        this.invoice = invoice;
        this.markingPaid = false;
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

  onRequestChanges(requestChangesRequest: RequestChangesPaymentRequestByUserDto): void {
    this.runStatusAction('requestChanges', 'Changes requested', 'Could not request changes: ', () =>
      this.service.requestChangesForPaymentRequestByUser(this.invoice!.id, requestChangesRequest),
    );
  }

  private runStatusAction(
    action: string,
    successMessage: string,
    errorPrefix: string,
    request: () => ReturnType<PaymentRequestByUserService['approvePaymentRequestByUser']>,
  ): void {
    if (!this.invoice || this.statusActionPending) return;

    this.statusActionPending = action;
    request().subscribe({
      next: (invoice) => {
        this.invoice = invoice;
        this.statusActionPending = null;
        this.notificationService.showSuccess(successMessage);
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.statusActionPending = null;
        this.notificationService.showError(errorPrefix + err.message);
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

  onBack(): void {
    this.router.navigate(['/requests']);
  }
}
