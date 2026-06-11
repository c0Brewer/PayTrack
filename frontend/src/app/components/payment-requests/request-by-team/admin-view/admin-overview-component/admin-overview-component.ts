import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import {
  GetPaymentRequestsByTeamOptions,
  PaymentRequestByTeamDto,
  TEAM_REQUEST_ALLOWED_STATUSES,
  TransactionStatus,
} from '../../../../../types/exporter';
import { StatBoxComponent } from '../../../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { TeamRequestAdminFilterComponent } from '../filter-component/filter-component';
import { TeamRequestAdminListComponent } from '../list-component/list-component';

@Component({
  selector: 'app-team-request-admin-overview-component',
  imports: [
    PaginationComponent,
    TeamRequestAdminFilterComponent,
    TeamRequestAdminListComponent,
    StatBoxComponent,
    EuroPipe,
  ],
  templateUrl: './admin-overview-component.html',
  styleUrl: './admin-overview-component.scss',
})
export class TeamRequestAdminOverviewComponent implements OnInit {
  constructor(
    private readonly paymentRequestByTeamService: PaymentRequestByTeamService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  requests: PaymentRequestByTeamDto[] = [];
  statRequests: PaymentRequestByTeamDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: GetPaymentRequestsByTeamOptions = {
    IncludeTeam: true,
  };

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    const countQuery: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      IncludeTeam: true,
      Limit: this.limit,
      Offset: 0,
    };

    this.paymentRequestByTeamService.getPaymentRequestsByTeam(countQuery).subscribe({
      next: (data) => {
        if (data?.items) {
          this.loadRequestStats(data.totalCount);
        } else {
          this.notificationService.showError('Error while loading payment requests');
        }
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  loadRequestStats(totalCount: number): void {
    if (totalCount <= 0) {
      this.statRequests = [];
      this.requests = [];
      this.totalCount = 0;
      this.hasNext = false;
      this.hasPrev = false;
      this.cdr.markForCheck();
      return;
    }

    const query: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      IncludeTeam: true,
      Limit: totalCount,
      Offset: 0,
    };

    this.paymentRequestByTeamService.getPaymentRequestsByTeam(query).subscribe({
      next: (data) => {
        this.statRequests = (data?.items ?? []).filter((request) =>
          TEAM_REQUEST_ALLOWED_STATUSES.includes(request.status as TransactionStatus),
        );

        const maxPage = Math.max(Math.ceil(this.statRequests.length / this.limit) - 1, 0);
        if (this.page > maxPage) {
          this.page = maxPage;
        }

        const offset = this.page * this.limit;
        this.requests = this.statRequests.slice(offset, offset + this.limit);
        this.totalCount = this.statRequests.length;
        this.hasPrev = this.page > 0;
        this.hasNext = offset + this.limit < this.totalCount;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  getTotalAmount(): number {
    return this.statRequests.reduce((total, request) => total + request.amount, 0);
  }

  getSubmittedRequestCount(): number {
    return this.statRequests.filter((request) => request.status === TransactionStatus.Submitted)
      .length;
  }

  getPaidRequestCount(): number {
    return this.statRequests.filter((request) => request.status === TransactionStatus.Paid).length;
  }

  updateFilterOptions(options: GetPaymentRequestsByTeamOptions): void {
    this.filterOptions = { ...this.filterOptions, ...options };
    this.page = 0;
    this.loadRequests();
  }

  onUpdateLimit(newLimit: number): void {
    this.limit = newLimit;
    this.page = 0;
    this.loadRequests();
  }

  onOpenDetail(request: PaymentRequestByTeamDto): void {
    this.router.navigate(['/payment-requests-by-team', request.id]);
  }

  getTotalPages(): number {
    const pages = Math.ceil(this.totalCount / this.limit);
    return pages > 0 ? pages : 1;
  }

  nextPage(): void {
    this.page++;
    this.loadRequests();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.loadRequests();
    }
  }

  navigateBankStatementUpload(): void {
    this.router.navigate(['/payment-requests-by-team/bank-statement-upload']);
  }
}
