import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import {
  PaymentRequestByUserDto,
  PayoutType,
  PayoutTypeLabels,
  SortDirection,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-invoice-list-component',
  imports: [DatePipe, EuroPipe],
  templateUrl: './list-component.html',
  styleUrl: './list-component.scss',
})
export class InvoiceListComponent {
  @Input() invoices: PaymentRequestByUserDto[] = [];
  @Input() showUserNameColumn: boolean = false;
  @Input() showDuplicateIndicator: boolean = false;
  @Input() sortBy: string | null = null;
  @Input() sortDirection: SortDirection | null = null;

  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();
  @Output() openDuplicates = new EventEmitter<PaymentRequestByUserDto>();
  @Output() sortChange = new EventEmitter<{ sortBy: string; sortDirection: SortDirection }>();

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.openDetail.emit(invoice);
  }

  onOpenDuplicates(invoice: PaymentRequestByUserDto): void {
    this.openDuplicates.emit(invoice);
  }

  onSort(sortBy: string): void {
    const sortDirection: SortDirection =
      this.sortBy === sortBy && this.sortDirection === 'Desc' ? 'Asc' : 'Desc';

    this.sortChange.emit({ sortBy, sortDirection });
  }

  getSortIcon(sortBy: string): string {
    if (this.sortBy !== sortBy) {
      return '';
    }

    return this.sortDirection === 'Asc' ? 'arrow_upward' : 'arrow_downward';
  }

  getPayoutTypeLabel(type: PayoutType): string {
    return PayoutTypeLabels[type] ?? 'Unknown';
  }

  getTransactionStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }
}
