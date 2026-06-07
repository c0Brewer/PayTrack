import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { EuroPipe } from '../../../../../pipes/euro.pipe';
import { DetailComponent } from '../../../../general/detail-component/detail-component';
import {
  PaymentRequestByTeamDto,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-team-request-team-detail-component',
  imports: [DatePipe, DetailComponent, EuroPipe],
  templateUrl: './detail-component.html',
  styleUrl: './detail-component.scss',
})
export class TeamRequestTeamDetailComponent {
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
