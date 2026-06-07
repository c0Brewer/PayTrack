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
  selector: 'app-team-request-admin-detail-view-component',
  imports: [DatePipe, EuroPipe],
  templateUrl: './detail-component.html',
  styleUrl: './detail-component.scss',
})
export class TeamRequestAdminDetailViewComponent {
  @Input() request: PaymentRequestByTeamDto | null = null;
  @Input() loading: boolean = false;
  @Output() back = new EventEmitter<void>();

  getStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status] ?? 'Unknown';
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status] ?? '';
  }
}
