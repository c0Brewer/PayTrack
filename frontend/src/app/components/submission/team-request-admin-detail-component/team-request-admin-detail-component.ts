import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto } from '../../../types/exporter';
import { TeamRequestDetailComponent } from '../team-request-detail-component/team-request-detail-component';

@Component({
  selector: 'app-team-request-admin-detail-component',
  imports: [TeamRequestDetailComponent],
  templateUrl: './team-request-admin-detail-component.html',
  styleUrl: './team-request-admin-detail-component.scss',
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

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));

      this.service
        .getPaymentRequestsByTeamById(id, {
          IncludeUser: true,
          IncludeTeam: true,
          IncludeCostCentre: true,
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
}
