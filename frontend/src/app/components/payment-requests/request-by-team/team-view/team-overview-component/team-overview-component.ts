import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { take } from 'rxjs';

import { AuthService } from '../../../../../services/auth/auth-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import {
  GetPaymentRequestsByTeamOptions,
  PaymentRequestByTeamDto,
  TransactionStatus,
  UserDto,
} from '../../../../../types/exporter';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { TeamRequestTeamFilterComponent } from '../filter-component/filter-component';
import { TeamRequestTeamListComponent } from '../list-component/list-component';
import { StatBoxComponent } from '../../../../general/boxes/stat-box-component/stat-box-component';
import { EuroPipe } from '../../../../../pipes/euro.pipe';

@Component({
  selector: 'app-team-request-team-overview-component',
  imports: [
    PaginationComponent,
    TeamRequestTeamFilterComponent,
    TeamRequestTeamListComponent,
    StatBoxComponent,
    EuroPipe,
  ],
  templateUrl: './team-overview-component.html',
  styleUrl: './team-overview-component.scss',
})
export class TeamRequestTeamOverviewComponent implements OnInit {
  constructor(
    private readonly paymentRequestByTeamService: PaymentRequestByTeamService,
    private readonly authService: AuthService,
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

  filterOptions: GetPaymentRequestsByTeamOptions = {};

  private currentUser: UserDto | null = null;

  ngOnInit(): void {
    this.authService
      .getCurrentUser()
      .pipe(take(1))
      .subscribe((user) => {
        this.currentUser = user;
        this.loadRequests();
      });
  }

  loadRequests(): void {
    const query: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      UserId: this.currentUser?.id,
      Limit: this.limit,
      Offset: this.page * this.limit,
    };

    this.paymentRequestByTeamService.getPaymentRequestsByTeam(query).subscribe({
      next: (data) => {
        if (data?.items) {
          this.requests = data.items;
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;
          this.loadRequestStats(data.totalCount);
          this.cdr.markForCheck();
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
      return;
    }

    const query: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      UserId: this.currentUser?.id,
      Limit: totalCount,
      Offset: 0,
    };

    this.paymentRequestByTeamService.getPaymentRequestsByTeam(query).subscribe({
      next: (data) => {
        this.statRequests = data?.items ?? [];
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
    this.router.navigate(['/my-team-requests', request.id]);
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
}
