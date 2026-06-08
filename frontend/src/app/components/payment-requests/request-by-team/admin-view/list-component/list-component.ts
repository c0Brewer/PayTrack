import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import {
  PaymentRequestByTeamDto,
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
