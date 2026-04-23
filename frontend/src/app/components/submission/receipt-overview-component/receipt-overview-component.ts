import { ChangeDetectorRef, Component } from '@angular/core';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestByUserDto } from '../../../types/exporter';

@Component({
  selector: 'app-receipt-overview-component',
  imports: [],
  templateUrl: './receipt-overview-component.html',
  styleUrl: './receipt-overview-component.scss',
})
export class ReceiptOverviewComponent {
  requests: PaymentRequestByUserDto[] = [];

  /*
   *
   * DISCLAIMER: THIS IS VERY HARD WORK IN PROGRESS.
   * This only exists to test the submission and should
   * be re-worked later.
   *
   */

  constructor(
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly notificationService: NotificationService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.paymentRequestService
      .getPaymentRequestsByUser({
        IncludeTeam: true,
        IncludeBankAccount: true,
      })
      .subscribe({
        next: (data) => {
          if (data.items == null) {
            this.notificationService.showError('Error loading request.');
            return;
          }

          this.requests = data.items;

          this.cdr.markForCheck();
        },
        error: (err) => {
          this.notificationService.showError(err);
        },
      });
  }
}
