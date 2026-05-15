import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
  PaymentRequestByTeamDto,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../types/exporter';

@Component({
  selector: 'app-team-request-detail-component',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './team-request-detail-component.html',
  styleUrl: './team-request-detail-component.scss',
})
export class TeamRequestDetailComponent {
  @Input() request: PaymentRequestByTeamDto | null = null;
  @Input() loading: boolean = false;
  @Input() showTeam: boolean = true;
  @Input() showCostCentre: boolean = true;
  @Input() showUser: boolean = true;
  @Output() back = new EventEmitter<void>();

  getStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }
}
