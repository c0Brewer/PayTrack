import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import {
  DuplicatePaymentRequestByUserDto,
  GetPaymentRequestsByUserOptions,
  PaymentRequestByUserDto,
} from '../../../types/exporter';
import { PaginationComponent } from '../../general/pagination-component/pagination-component';
import { DuplicateListModalComponent } from '../duplicate-list-modal-component/duplicate-list-modal-component';
import { InvoiceFilterComponent } from '../invoice-filter-component/invoice-filter-component';
import { InvoiceListComponent } from '../invoice-list-component/invoice-list-component';

@Component({
  selector: 'app-requests-component',
  imports: [
    PaginationComponent,
    InvoiceFilterComponent,
    InvoiceListComponent,
    DuplicateListModalComponent,
  ],
  templateUrl: './requests-component.html',
  styleUrl: './requests-component.scss',
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

  selectedDuplicateInvoice: PaymentRequestByUserDto | null = null;
  duplicateCandidates: DuplicatePaymentRequestByUserDto[] = [];
  isDuplicateModalOpen: boolean = false;
  isDuplicateModalLoading: boolean = false;
  duplicateActionInvoiceId: number | null = null;

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

    if (!window.confirm(`Delete invoice ${invoice.invoiceNumber}?`)) {
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
            (candidate) =>
              candidate.paymentRequestByUser.id !== duplicate.paymentRequestByUser.id,
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
}
