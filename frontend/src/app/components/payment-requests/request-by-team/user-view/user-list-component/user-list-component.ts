import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { take } from 'rxjs';

import { AuthService } from '../../../../../services/auth/auth-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import {
  GetPaymentRequestsByTeamOptions,
  PaymentRequestByTeamDto,
  SortDirection,
  UserDto,
} from '../../../../../types/exporter';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { TeamRequestFilterComponent } from '../../general/filter-component/filter-component';
import { TeamRequestListComponent } from '../../general/list-component/list-component';

@Component({
  selector: 'app-team-request-user-list-component',
  imports: [PaginationComponent, TeamRequestFilterComponent, TeamRequestListComponent],
  templateUrl: './user-list-component.html',
  styleUrl: './user-list-component.scss',
})
export class TeamRequestUserListComponent implements OnInit {
  constructor(
    private readonly paymentRequestByTeamService: PaymentRequestByTeamService,
    private readonly authService: AuthService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  requests: PaymentRequestByTeamDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: GetPaymentRequestsByTeamOptions = {};
  sortBy: string | null = null;
  sortDirection: SortDirection | null = null;

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
      SortBy: this.sortBy ?? undefined,
      SortDirection: this.sortDirection ?? undefined,
    };

    this.paymentRequestByTeamService.getPaymentRequestsByTeam(query).subscribe({
      next: (data) => {
        if (data?.items) {
          this.requests = data.items;
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;
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

  updateFilterOptions(options: GetPaymentRequestsByTeamOptions): void {
    this.filterOptions = { ...this.filterOptions, ...options };
    this.page = 0;
    this.loadRequests();
  }

  onSortChange(sort: { sortBy: string; sortDirection: SortDirection }): void {
    this.sortBy = sort.sortBy;
    this.sortDirection = sort.sortDirection;
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
