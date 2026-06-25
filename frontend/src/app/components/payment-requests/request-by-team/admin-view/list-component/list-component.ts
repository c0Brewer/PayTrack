import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import {
  PaymentRequestByTeamDto,
  SortDirection,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-team-request-admin-list-component',
  imports: [DatePipe, EuroPipe],
  templateUrl: './list-component.html',
  styleUrl: './list-component.scss',
})
export class TeamRequestAdminListComponent {
  @Input() requests: PaymentRequestByTeamDto[] = [];
  @Input() showTeamColumn: boolean = true;
  @Input() showUserColumn: boolean = true;
  @Input() sortBy: string | null = null;
  @Input() sortDirection: SortDirection | null = null;

  @Output() openDetail = new EventEmitter<PaymentRequestByTeamDto>();
  @Output() sortChange = new EventEmitter<{ sortBy: string; sortDirection: SortDirection }>();

  onOpenDetail(request: PaymentRequestByTeamDto): void {
    this.openDetail.emit(request);
  }

  onSort(sortBy: string): void {
    const sortDirection: SortDirection =
      this.sortBy === sortBy && this.sortDirection === 'Asc' ? 'Desc' : 'Asc';

    this.sortChange.emit({ sortBy, sortDirection });
  }

  getSortIcon(sortBy: string): string {
    if (this.sortBy !== sortBy) {
      return '';
    }

    return this.sortDirection === 'Asc' ? 'arrow_upward' : 'arrow_downward';
  }

  getTransactionStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }
}
