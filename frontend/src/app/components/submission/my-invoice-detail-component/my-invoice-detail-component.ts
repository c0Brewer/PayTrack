import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestByUserDto } from '../../../types/exporter';
import { InvoiceDetailComponent } from '../invoice-detail-component/invoice-detail-component';

@Component({
  selector: 'app-my-invoice-detail-component',
  imports: [InvoiceDetailComponent],
  templateUrl: './my-invoice-detail-component.html',
  styleUrl: './my-invoice-detail-component.scss',
})
export class MyInvoiceDetailComponent implements OnInit, OnDestroy {
  constructor(
    private readonly service: PaymentRequestByUserService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) { }

  invoice: PaymentRequestByUserDto | null = null;
  receiptBlobUrl: string | null = null;
  rawReceiptBlobUrl: string | null = null;
  receiptMimeType: string = '';
  isReceiptImage: boolean = false;
  loading: boolean = true;

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.service
        .getMyInvoiceById(id, {
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

      this.service.downloadMyReceipt(id).subscribe({
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
}
