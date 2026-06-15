import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { FinancialExportService } from '../../../../../services/financial-export/financial-export-service';
import { NotificationService } from '../../../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  DuplicatePaymentRequestByUserDto,
  FinancialExportFormat,
  FinancialExportSource,
  GetPaymentRequestsByUserOptions,
  PaymentRequestByUserDto,
  TransactionStatus,
  SortDirection,
} from '../../../../../types/exporter';
import { StatBoxComponent } from '../../../../general/boxes/stat-box-component/stat-box-component';
import { PaginationComponent } from '../../../../general/pagination-component/pagination-component';
import { DuplicateListModalComponent } from '../../duplicate-list-modal-component/duplicate-list-modal-component';
import { AdminInvoiceFilterComponent } from '../filter-component/filter-component';
import { AdminInvoiceListComponent } from '../list-component/list-component';

@Component({
  selector: 'app-admin-invoices-overview-component',
  imports: [
    PaginationComponent,
    AdminInvoiceFilterComponent,
    AdminInvoiceListComponent,
    DuplicateListModalComponent,
    EuroPipe,
    StatBoxComponent,
  ],
  templateUrl: './admin-overview-component.html',
  styleUrl: './admin-overview-component.scss',
})
export class AdminInvoicesOverviewComponent implements OnInit {
  constructor(
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly financialExportService: FinancialExportService,
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

  selectedDuplicateInvoice: PaymentRequestByUserDto | null = null;
  duplicateCandidates: DuplicatePaymentRequestByUserDto[] = [];
  isDuplicateModalOpen: boolean = false;
  isDuplicateModalLoading: boolean = false;
  duplicateActionInvoiceId: number | null = null;
  isExporting: boolean = false;
  sortBy: string | null = null;
  sortDirection: SortDirection | null = null;
  FinancialExportFormat = FinancialExportFormat;

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    const query: GetPaymentRequestsByUserOptions = {
      ...this.filterOptions,
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

  getDeclinedRequestCount(): number {
    return this.statInvoices.filter((invoice) => invoice.status === TransactionStatus.Declined)
      .length;
  }

  getPaidRequestCount(): number {
    return this.statInvoices.filter((invoice) => invoice.status === TransactionStatus.Paid).length;
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

  exportFinancialData(format: FinancialExportFormat): void {
    this.isExporting = true;

    this.financialExportService
      .downloadFinancialData(
        {
          ...this.filterOptions,
          Source: FinancialExportSource.SubmittedInvoices,
          Limit: undefined,
          Offset: undefined,
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

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.router.navigate(['/requests', invoice.id]);
  }

  onOpenDuplicates(invoice: PaymentRequestByUserDto): void {
    if (!invoice.team?.id || !invoice.paidAt) {
      this.notificationService.showError('Duplicate lookup is missing team or paid date.');
      return;
    }

    this.selectedDuplicateInvoice = invoice;
    this.duplicateCandidates = [];
    this.isDuplicateModalOpen = true;
    this.isDuplicateModalLoading = true;

    this.paymentRequestService
      .getDuplicatePaymentRequestsByUser({
        TeamId: invoice.team.id,
        Amount: invoice.amount,
        PaidAt: invoice.paidAt,
        InvoiceNumber: invoice.invoiceNumber,
        PaymentRequestByUserId: invoice.id,
      })
      .subscribe({
        next: (duplicates) => {
          this.duplicateCandidates = duplicates.filter(
            (duplicate) => duplicate.paymentRequestByUser.id !== invoice.id,
          );
          this.isDuplicateModalLoading = false;
          this.cdr.markForCheck();
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Duplicate check failed.');
          this.isDuplicateModalLoading = false;
          this.isDuplicateModalOpen = false;
          this.cdr.markForCheck();
        },
      });
  }

  onCloseDuplicateModal(): void {
    this.isDuplicateModalOpen = false;
    this.isDuplicateModalLoading = false;
    this.duplicateActionInvoiceId = null;
    this.selectedDuplicateInvoice = null;
    this.duplicateCandidates = [];
  }

  onOpenDuplicateDetail(invoice: PaymentRequestByUserDto): void {
    this.onCloseDuplicateModal();
    this.onOpenDetail(invoice);
  }

  onDeleteDuplicateInvoice(invoice: PaymentRequestByUserDto): void {
    if (!invoice.id) {
      this.notificationService.showError('Invoice id is missing.');
      return;
    }

    this.duplicateActionInvoiceId = invoice.id;

    this.paymentRequestService.deletePaymentRequestByUser(invoice.id).subscribe({
      next: () => {
        this.notificationService.showSuccess('Invoice deleted.');
        this.onCloseDuplicateModal();
        this.loadInvoices();
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.notificationService.showError(err.message ?? 'Deleting invoice failed.');
        this.duplicateActionInvoiceId = null;
        this.cdr.markForCheck();
      },
    });
  }

  onDismissDuplicate(duplicate: DuplicatePaymentRequestByUserDto): void {
    if (!this.selectedDuplicateInvoice?.id || !duplicate.paymentRequestByUser.id) {
      this.notificationService.showError('Duplicate warning cannot be dismissed.');
      return;
    }

    this.duplicateActionInvoiceId = duplicate.paymentRequestByUser.id;

    this.paymentRequestService
      .dismissDuplicatePaymentRequestByUser(
        this.selectedDuplicateInvoice.id,
        duplicate.paymentRequestByUser.id,
      )
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Duplicate warning dismissed.');
          this.duplicateCandidates = this.duplicateCandidates.filter(
            (candidate) => candidate.paymentRequestByUser.id !== duplicate.paymentRequestByUser.id,
          );
          this.duplicateActionInvoiceId = null;

          if (this.duplicateCandidates.length === 0) {
            this.onCloseDuplicateModal();
          }

          this.loadInvoices();
          this.cdr.markForCheck();
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Dismissing duplicate warning failed.');
          this.duplicateActionInvoiceId = null;
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
    this.loadInvoices();
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.loadInvoices();
    }
  }

  navigateBankStatementUpload(): void {
    this.router.navigate(['/bank-statement-upload'], {
      state: {
        returnTo: '/requests',
        backLabel: 'Back to Invoices',
      },
    });
  }
}
