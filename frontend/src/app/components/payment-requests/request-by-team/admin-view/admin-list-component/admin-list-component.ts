import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { FinancialExportService } from '../../../../../services/financial-export/financial-export-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../../services/payment-request-by-team/payment-request-by-team-service';
import {
  FinancialExportFormat,
  FinancialExportSource,
  GetPaymentRequestsByTeamOptions,
  PaymentRequestByTeamDto,
  SortDirection,
  TEAM_REQUEST_ALLOWED_STATUSES,
  TransactionStatus,
} from '../../../../../types/exporter';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { TeamRequestFilterComponent } from '../../general/filter-component/filter-component';
import { TeamRequestListComponent } from '../../general/list-component/list-component';

@Component({
  selector: 'app-team-requests-component',
  imports: [PaginationComponent, TeamRequestFilterComponent, TeamRequestListComponent],
  templateUrl: './admin-list-component.html',
  styleUrl: './admin-list-component.scss',
})
export class TeamRequestsComponent implements OnInit {
  constructor(
    private readonly paymentRequestByTeamService: PaymentRequestByTeamService,
    private readonly financialExportService: FinancialExportService,
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

  filterOptions: GetPaymentRequestsByTeamOptions = {
    IncludeTeam: true,
  };
  isExporting: boolean = false;
  sortBy: string | null = null;
  sortDirection: SortDirection | null = null;
  FinancialExportFormat = FinancialExportFormat;

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    const query: GetPaymentRequestsByTeamOptions = {
      ...this.filterOptions,
      IncludeTeam: true,
      Limit: this.limit,
      Offset: this.page * this.limit,
      SortBy: this.sortBy ?? undefined,
      SortDirection: this.sortDirection ?? undefined,
    };

    this.paymentRequestByTeamService.getPaymentRequestsByTeam(query).subscribe({
      next: (data) => {
        if (data?.items) {
          this.requests = data.items.filter((r) =>
            TEAM_REQUEST_ALLOWED_STATUSES.includes(r.status as TransactionStatus),
          );
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

  exportFinancialData(format: FinancialExportFormat): void {
    this.isExporting = true;

    this.financialExportService
      .downloadFinancialData(
        {
          ...this.filterOptions,
          Source: FinancialExportSource.PaymentRequests,
          Limit: undefined,
          Offset: undefined,
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
    this.router.navigate(['/bank-statement-upload']);
  }
}
