import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { AuthService } from '../../../../../services/auth/auth-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  GetPaymentRequestsByUserOptions,
  PaymentRequestByUserDto,
  PayoutType,
  SortDirection,
  TransactionStatus,
  UserDto,
} from '../../../../../types/exporter';
import { StatBoxComponent } from '../../../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { UserInvoiceFilterComponent } from '../filter-component/filter-component';
import { UserInvoiceListComponent } from '../list-component/list-component';

@Component({
  selector: 'app-user-invoices-overview-component',
  imports: [
    PaginationComponent,
    UserInvoiceFilterComponent,
    UserInvoiceListComponent,
    StatBoxComponent,
    EuroPipe,
  ],
  templateUrl: './user-overview-component.html',
  styleUrl: './user-overview-component.scss',
})
export class UserInvoicesOverviewComponent implements OnInit {
  constructor(
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly authService: AuthService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  invoices: PaymentRequestByUserDto[] = [];
  statInvoices: PaymentRequestByUserDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: GetPaymentRequestsByUserOptions = {
    IncludeTeam: true,
  };
  sortBy: string | null = null;
  sortDirection: SortDirection | null = null;

  private currentUser: UserDto | null = null;

  ngOnInit(): void {
    this.authService
      .getCurrentUser()
      .pipe(take(1))
      .subscribe((user) => {
        this.currentUser = user;
        this.filterOptions = {
          IncludeTeam: true,
          ...this.getFilterOptionsFromQueryParams(),
        };
        this.loadInvoices();
      });
  }

  loadInvoices(): void {
    const query: GetPaymentRequestsByUserOptions = {
      ...this.filterOptions,
      UserId: this.currentUser?.id,
      IncludeTeam: true,
      Limit: this.limit,
      Offset: this.page * this.limit,
      SortBy: this.sortBy ?? undefined,
      SortDirection: this.sortDirection ?? undefined,
    };

    this.paymentRequestService.getPaymentRequestsByUser(query).subscribe({
      next: (data) => {
        if (data?.items) {
          this.invoices = data.items;
          this.totalCount = data.totalCount;
          this.hasNext = data.hasNext ?? false;
          this.hasPrev = data.hasPrevious ?? false;
          this.loadInvoiceStats(data.totalCount);
          this.cdr.markForCheck();
        } else {
          this.notificationService.showError('Error while loading invoices');
        }
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  loadInvoiceStats(totalCount: number): void {
    if (totalCount <= 0) {
      this.statInvoices = [];
      return;
    }

    const query: GetPaymentRequestsByUserOptions = {
      ...this.filterOptions,
      UserId: this.currentUser?.id,
      IncludeTeam: true,
      Limit: totalCount,
      Offset: 0,
    };

    this.paymentRequestService.getPaymentRequestsByUser(query).subscribe({
      next: (data) => {
        this.statInvoices = data?.items ?? [];
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.notificationService.showError(err);
      },
    });
  }

  getTotalAmount(): number {
    return this.statInvoices.reduce((total, invoice) => total + invoice.amount, 0);
  }

  getPaidInvoiceCount(): number {
    return this.statInvoices.filter((invoice) => invoice.status === TransactionStatus.Paid).length;
  }

  getOpenInvoiceCount(): number {
    return this.statInvoices.filter(
      (invoice) =>
        invoice.status !== TransactionStatus.Paid && invoice.status !== TransactionStatus.Declined,
    ).length;
  }

  updateFilterOptions(options: GetPaymentRequestsByUserOptions): void {
    this.filterOptions = { ...this.filterOptions, ...options };
    this.page = 0;
    this.loadInvoices();
  }

  onSortChange(sort: { sortBy: string; sortDirection: SortDirection }): void {
    this.sortBy = sort.sortBy;
    this.sortDirection = sort.sortDirection;
    this.page = 0;
    this.loadInvoices();
  }

  onUpdateLimit(newLimit: number): void {
    this.limit = newLimit;
    this.page = 0;
    this.loadInvoices();
  }

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.router.navigate(['/my-invoices', invoice.id]);
  }

  getTotalPages(): number {
    const pages = Math.ceil(this.totalCount / this.limit);
    return pages > 0 ? pages : 1;
  }

  nextPage(): void {
    this.page++;
    this.loadInvoices();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.loadInvoices();
    }
  }

  private getFilterOptionsFromQueryParams(): GetPaymentRequestsByUserOptions {
    const queryParams = this.route.snapshot.queryParams;

    return {
      InvoiceNumber: this.getStringQueryParam(queryParams['invoiceNumber']),
      Status: this.getNumberQueryParam(queryParams['status']) as TransactionStatus | undefined,
      MinCreatedAt: this.getStringQueryParam(queryParams['minCreatedAt']),
      MaxCreatedAt: this.getStringQueryParam(queryParams['maxCreatedAt']),
      MinPaidAt: this.getStringQueryParam(queryParams['minPaidAt']),
      MaxPaidAt: this.getStringQueryParam(queryParams['maxPaidAt']),
      MinAmount: this.getNumberQueryParam(queryParams['minAmount']),
      MaxAmount: this.getNumberQueryParam(queryParams['maxAmount']),
      PurposeOfPayment: this.getStringQueryParam(queryParams['purposeOfPayment']),
      TeamId: this.getNumberQueryParam(queryParams['teamId']),
      PayoutType: this.getNumberQueryParam(queryParams['payoutType']) as PayoutType | undefined,
    };
  }

  private getStringQueryParam(value: unknown): string | undefined {
    return typeof value === 'string' && value.trim() !== '' ? value : undefined;
  }

  private getNumberQueryParam(value: unknown): number | undefined {
    if (typeof value !== 'string' || value.trim() === '') {
      return undefined;
    }

    const parsedValue = Number(value);
    return Number.isFinite(parsedValue) ? parsedValue : undefined;
  }
}
