import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto } from '../../../../../types/exporter';
import { ExternalNotificationComponent } from '../../../../general/external-notification-component/external-notification-component';
import { TeamRequestDetailComponent } from '../../general/detail-component/detail-component';

@Component({
  selector: 'app-team-request-admin-detail-component',
  imports: [TeamRequestDetailComponent, ExternalNotificationComponent],
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

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

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
    const amount = this.request.amount.toLocaleString('de-DE', {
      style: 'currency',
      currency: 'EUR',
    });
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
}
