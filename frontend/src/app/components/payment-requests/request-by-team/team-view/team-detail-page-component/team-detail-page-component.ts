import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByTeamDto } from '../../../../../types/exporter';
import { TeamRequestTeamDetailComponent } from '../detail-component/detail-component';

@Component({
  selector: 'app-team-request-team-detail-page-component',
  imports: [TeamRequestTeamDetailComponent],
  templateUrl: './team-detail-page-component.html',
  styleUrl: './team-detail-page-component.scss',
})
export class TeamRequestTeamDetailPageComponent implements OnInit {
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
          IncludeUser: false,
          IncludeTeam: false,
          IncludeStatusHistory: false,
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
    this.router.navigate(['/my-team-requests']);
  }
}
