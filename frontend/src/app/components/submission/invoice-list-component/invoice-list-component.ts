import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { PaymentRequestByUserDto } from '../../../types/exporter';

@Component({
  selector: 'app-invoice-list-component',
  imports: [DatePipe],
  templateUrl: './invoice-list-component.html',
  styleUrl: './invoice-list-component.scss',
})
export class InvoiceListComponent {
  @Input() invoices: PaymentRequestByUserDto[] = [];
  @Input() showCostCentreColumn: boolean = false;
  @Input() showUserNameColumn: boolean = false;

  @Output() openDetail = new EventEmitter<PaymentRequestByUserDto>();

  onOpenDetail(invoice: PaymentRequestByUserDto): void {
    this.openDetail.emit(invoice);
  }

  payoutTypeToText(payoutType: 0 | 1): string {
    return payoutType === 0 ? 'Internal' : 'External';
  }

  statusToText(status: 0 | 1 | 2 | 3 | 4): string {
    switch (status) {
      case 0:
        return 'Submitted';
      case 1:
        return 'Changes requested';
      case 2:
        return 'Approved';
      case 3:
        return 'Paid';
      case 4:
        return 'Declined';
      default:
        return 'Unknown';
    }
  }
}
