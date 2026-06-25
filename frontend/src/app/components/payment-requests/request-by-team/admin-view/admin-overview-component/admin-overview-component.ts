import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { FinancialExportService } from '../../../../../services/financial-export/financial-export-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import {
  FinancialExportFormat,
  FinancialExportSource,
  GetPaymentRequestsByTeamOptions,
  PaymentRequestByTeamDto,
  SortDirection,
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
    private readonly financialExportService: FinancialExportService,
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
  isExporting: boolean = false;
  sortBy: string | null = null;
  sortDirection: SortDirection | null = null;
  FinancialExportFormat = FinancialExportFormat;

  filterOptions: GetPaymentRequestsByTeamOptions = {
    IncludeTeam: true,
    VisibleStatusesOnly: true,
  };

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    const query: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      IncludeTeam: true,
      VisibleStatusesOnly: true,
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
      this.cdr.markForCheck();
      return;
    }

    const query: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      IncludeTeam: true,
      VisibleStatusesOnly: true,
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
    this.filterOptions = { ...this.filterOptions, ...options, VisibleStatusesOnly: true };
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
    this.router.navigate(['/payment-requests-by-team', request.id]);
  }

  exportFinancialData(format: FinancialExportFormat): void {
    this.isExporting = true;

    this.financialExportService
      .downloadFinancialData(
        {
          ...this.filterOptions,
          Source: FinancialExportSource.PaymentRequests,
          Limit: undefined,
          Offset: undefined,
          VisibleStatusesOnly: true,
          SortBy: this.sortBy ?? undefined,
          SortDirection: this.sortDirection ?? undefined,
        },
        format,
      )
      .subscribe({
        next: () => {
          this.isExporting = false;
          this.notificationService.showSuccess('Financial export downloaded.');
          this.cdr.markForCheck();
        },
        error: (err: Error) => {
          this.isExporting = false;
          this.notificationService.showError(err.message ?? 'Financial export failed.');
          this.cdr.markForCheck();
        },
      });
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
    this.router.navigate(['/bank-statement-upload'], {
      state: {
        returnTo: '/payment-requests-by-team',
        backLabel: 'Back to Payment Requests',
      },
    });
  }
}
