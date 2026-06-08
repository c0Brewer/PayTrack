import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';

import {
  GetPaymentRequestsByTeamOptions,
  TEAM_REQUEST_ALLOWED_STATUSES,
  TransactionStatus,
  TransactionStatusLabels,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-team-request-team-filter-component',
  imports: [FormsModule],
  templateUrl: './filter-component.html',
  styleUrl: './filter-component.scss',
})
export class TeamRequestTeamFilterComponent implements OnInit {
  @Input() limitSelection: number[] = [];
  @Input() limit: number = 10;

  @Output() updateFilter = new EventEmitter<GetPaymentRequestsByTeamOptions>();
  @Output() limitChange = new EventEmitter<number>();

  filterPurpose: string = '';
  filterMinAmount: string = '';
  filterMaxAmount: string = '';
  filterMinDueDate: string = '';
  filterMaxDueDate: string = '';
  filterStatus: TransactionStatus | undefined = undefined;

  private readonly filterPurposeSubject = new Subject<string>();
  private readonly filterMinAmountSubject = new Subject<string>();
  private readonly filterMaxAmountSubject = new Subject<string>();
  private readonly filterMinDueDateSubject = new Subject<string>();
  private readonly filterMaxDueDateSubject = new Subject<string>();
  private readonly filterStatusSubject = new Subject<TransactionStatus | undefined>();

  TransactionStatusLabels = TransactionStatusLabels;
  transactionStatusOptions: TransactionStatus[] = [...TEAM_REQUEST_ALLOWED_STATUSES];

  ngOnInit(): void {
    this.filterPurposeSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterPurpose = value;
      this.emitFilter();
    });

    this.filterMinAmountSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMinAmount = value;
      this.emitFilter();
    });

    this.filterMaxAmountSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMaxAmount = value;
      this.emitFilter();
    });

    this.filterMinDueDateSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMinDueDate = value;
      this.emitFilter();
    });

    this.filterMaxDueDateSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMaxDueDate = value;
      this.emitFilter();
    });

    this.filterStatusSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterStatus = value;
      this.emitFilter();
    });
  }

  emitFilter(): void {
    this.updateFilter.emit(this.getFilterOptions());
  }

  getFilterOptions(): GetPaymentRequestsByTeamOptions {
    return {
      PurposeOfPayment: this.filterPurpose || undefined,
      MinAmount: this.filterMinAmount ? Number(this.filterMinAmount) : undefined,
      MaxAmount: this.filterMaxAmount ? Number(this.filterMaxAmount) : undefined,
      MinDueDate: this.filterMinDueDate || undefined,
      MaxDueDate: this.filterMaxDueDate || undefined,
      Status: this.filterStatus,
      Limit: undefined,
      Offset: undefined,
    };
  }

  onPurposeChange(event: Event): void {
    this.filterPurposeSubject.next((event.target as HTMLInputElement).value);
  }

  onMinAmountChange(event: Event): void {
    this.filterMinAmountSubject.next((event.target as HTMLInputElement).value);
  }

  onMaxAmountChange(event: Event): void {
    this.filterMaxAmountSubject.next((event.target as HTMLInputElement).value);
  }

  onMinDueDateChange(event: Event): void {
    this.filterMinDueDateSubject.next((event.target as HTMLInputElement).value);
  }

  onMaxDueDateChange(event: Event): void {
    this.filterMaxDueDateSubject.next((event.target as HTMLInputElement).value);
  }

  onStatusChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterStatusSubject.next(value !== '' ? Number(value) : undefined);
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
