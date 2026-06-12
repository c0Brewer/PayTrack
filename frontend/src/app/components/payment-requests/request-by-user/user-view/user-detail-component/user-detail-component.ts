import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestByUserDto, TransactionStatus } from '../../../../../types/exporter';
import { InvoiceDetailComponent } from '../../general/detail-component/detail-component';

@Component({
  selector: 'app-my-invoice-detail-component',
  imports: [InvoiceDetailComponent],
  templateUrl: './user-detail-component.html',
  styleUrl: './user-detail-component.scss',
})
export class MyInvoiceDetailComponent implements OnInit, OnDestroy {
  constructor(
    private readonly service: PaymentRequestByUserService,
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

  get latestChangeRequestMessage(): string | null {
    if (this.invoice?.status !== TransactionStatus.ChangesRequested) {
      return null;
    }

    const latestChangeRequest = [...(this.invoice.statusHistory ?? [])]
      .filter(
        (entry) => entry.toStatus === TransactionStatus.ChangesRequested && !!entry.comment?.trim(),
      )
      .sort(
        (left, right) => new Date(right.changedAt).getTime() - new Date(left.changedAt).getTime(),
      )[0];

    return latestChangeRequest?.comment?.trim() ?? null;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.service
        .getPaymentRequestsByUserById(id, {
          IncludeTeam: true,
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

  private getExtensionFromMimeType(mimeType: string): string {
    const map: Record<string, string> = {
      'image/jpeg': '.jpg',
      'image/png': '.png',
      'application/pdf': '.pdf',
    };
    return map[mimeType] ?? '';
  }

  onBack(): void {
    this.router.navigate(['/my-invoices']);
  }

  onEdit(): void {
    if (this.invoice?.status === TransactionStatus.ChangesRequested) {
      this.router.navigate(['/my-invoices', this.invoice.id, 'edit']);
    }
  }
}
