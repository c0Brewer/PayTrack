import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  GetPaymentRequestsByUserOptions,
  PaymentRequestByUserDto,
} from '../../../../../types/exporter';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { InvoiceFilterComponent } from '../../general/filter-component/filter-component';
import { InvoiceListComponent } from '../../general/list-component/list-component';

@Component({
  selector: 'app-requests-component',
  imports: [PaginationComponent, InvoiceFilterComponent, InvoiceListComponent],
  templateUrl: './admin-list-component.html',
  styleUrl: './admin-list-component.scss',
})
export class RequestsComponent implements OnInit {
  constructor(
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  invoices: PaymentRequestByUserDto[] = [];

  limitSelection: number[] = [10, 25, 50];

  limit: number = this.limitSelection[0];
  page: number = 0;
  totalCount: number = 0;
  hasNext: boolean = false;
  hasPrev: boolean = false;

  filterOptions: GetPaymentRequestsByUserOptions = {
    IncludeTeam: true,
    IncludeCostCentre: true,
  };

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    const query: GetPaymentRequestsByUserOptions = {
      ...this.filterOptions,
      IncludeTeam: true,
      IncludeCostCentre: true,
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

  updateFilterOptions(options: GetPaymentRequestsByUserOptions): void {
    this.filterOptions = { ...this.filterOptions, ...options };
    this.page = 0;
    this.loadInvoices();
  }

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.router.navigate(['/requests', invoice.id]);
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
