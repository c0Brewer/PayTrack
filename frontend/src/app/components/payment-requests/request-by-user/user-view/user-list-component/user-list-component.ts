import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { take } from 'rxjs';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { AuthService } from '../../../../../services/auth/auth-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  GetPaymentRequestsByUserOptions,
  PaymentRequestByUserDto,
  TransactionStatus,
  UserDto,
} from '../../../../../types/exporter';
import { StatBoxComponent } from '../../../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { InvoiceFilterComponent } from '../../general/filter-component/filter-component';
import { InvoiceListComponent } from '../../general/list-component/list-component';

@Component({
  selector: 'app-my-invoices-component',
  imports: [
    PaginationComponent,
    InvoiceFilterComponent,
    InvoiceListComponent,
    StatBoxComponent,
    EuroPipe,
  ],
  templateUrl: './user-list-component.html',
  styleUrl: './user-list-component.scss',
})
export class MyInvoicesComponent implements OnInit {
  constructor(
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly authService: AuthService,
    private readonly notificationService: NotificationService,
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

  private currentUser: UserDto | null = null;

  ngOnInit(): void {
    this.authService
      .getCurrentUser()
      .pipe(take(1))
      .subscribe((user) => {
        this.currentUser = user;
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
}
