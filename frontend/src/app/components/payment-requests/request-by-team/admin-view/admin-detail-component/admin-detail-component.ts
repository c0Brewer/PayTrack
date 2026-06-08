import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto, TransactionStatus } from '../../../../../types/exporter';
import { ExternalNotificationComponent } from '../../../../general/external-notification-component/external-notification-component';
import { ModalComponent } from '../../../../general/modal-component/modal-component';
import { TeamRequestAdminDetailViewComponent } from '../detail-component/detail-component';

@Component({
  selector: 'app-team-request-admin-detail-component',
  imports: [
    TeamRequestAdminDetailViewComponent,
    ExternalNotificationComponent,
    FormsModule,
    ModalComponent,
  ],
  templateUrl: './admin-detail-component.html',
  styleUrl: './admin-detail-component.scss',
})
export class TeamRequestAdminDetailComponent implements OnInit {
  constructor(
    private readonly service: PaymentRequestByTeamService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  request: PaymentRequestByTeamDto | null = null;
  loading: boolean = true;
  modalType: 'email' | 'slack' | null = null;

  showMarkAsPaidModal = false;
  markAsPaidComment = 'Payment manually approved and processed.';
  markAsPaidLoading = false;

  protected readonly transactionStatus = TransactionStatus;

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      this.loadRequest(id);
    });
  }

  onBack(): void {
    this.router.navigate(['/payment-requests-by-team']);
  }

  openEmailModal(): void {
    this.modalType = 'email';
  }

  openSlackModal(): void {
    this.modalType = 'slack';
  }

  get canMarkAsPaid(): boolean {
    const s = this.request?.status;
    return s !== undefined && s !== TransactionStatus.Paid && s !== TransactionStatus.Declined;
  }

  openMarkAsPaidModal(): void {
    this.markAsPaidComment = 'Payment manually approved and processed.';
    this.showMarkAsPaidModal = true;
  }

  cancelMarkAsPaid(): void {
    this.showMarkAsPaidModal = false;
  }

  confirmMarkAsPaid(): void {
    if (!this.request) return;
    this.markAsPaidLoading = true;
    this.service.markAsPaid(this.request.id, { comment: this.markAsPaidComment }).subscribe({
      next: (updated) => {
        this.notificationService.showSuccess('Payment marked as paid.');
        this.markAsPaidLoading = false;
        this.showMarkAsPaidModal = false;
        this.request = { ...this.request!, status: updated.status, paidAt: updated.paidAt };
        this.cdr.detectChanges();
      },
      error: (err: Error) => {
        this.notificationService.showError('Could not mark as paid: ' + err.message);
        this.markAsPaidLoading = false;
      },
    });
  }

  get notificationEmail(): string {
    return this.request?.user?.email ?? '';
  }

  get notificationSubject(): string {
    return `Payment Reminder – Request #${this.request?.id}`;
  }

  get notificationMessage(): string {
    if (!this.request) return '';
    const name = this.request.user?.name ?? 'User';
    const id = this.request.id;
    const amount = new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(
      this.request.amount,
    );
    const dueDate = this.request.dueDate
      ? new Date(this.request.dueDate).toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'long',
          day: 'numeric',
        })
      : 'N/A';

    if (this.modalType === 'email') {
      return (
        `Dear ${name},\n\n` +
        `This is a reminder for payment request #${id} for ${amount}, due ${dueDate}.\n` +
        `Please process this request at your earliest convenience.\n\n` +
        `Best regards,\nPayTrack`
      );
    }
    return (
      `Reminder: Payment request #${id} for ${amount} (due ${dueDate}) ` +
      `requires your attention. Please process at your earliest convenience.`
    );
  }

  private loadRequest(id: number): void {
    this.loading = true;
    this.service
      .getPaymentRequestsByTeamById(id, {
        IncludeUser: true,
        IncludeTeam: true,
        IncludeStatusHistory: true,
      })
      .subscribe({
        next: (data) => {
          this.request = data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load payment request: ' + err.message);
          this.loading = false;
        },
      });
  }
}
