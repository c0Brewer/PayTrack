import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
  PaymentRequestByTeamDto,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-team-request-list-component',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './list-component.html',
  styleUrl: './list-component.scss',
})
export class TeamRequestListComponent {
  @Input() requests: PaymentRequestByTeamDto[] = [];
  @Input() showTeamColumn: boolean = true;
  @Input() showBudgetColumn: boolean = true;
  @Input() showUserColumn: boolean = true;

  @Output() openDetail = new EventEmitter<PaymentRequestByTeamDto>();

  onOpenDetail(request: PaymentRequestByTeamDto): void {
    this.openDetail.emit(request);
  }

  getTransactionStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }
}
