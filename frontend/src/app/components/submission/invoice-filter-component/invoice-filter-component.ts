import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';

import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { TeamService } from '../../../services/team/team-service';
import { UserService } from '../../../services/user/user-service';
import {
  GetPaymentRequestsByUserOptions,
  TeamDto,
  TransactionStatus,
  TransactionStatusLabels,
  CostCentreDto,
  PayoutType,
  PayoutTypeLabels,
  UserDto,
} from '../../../types/exporter';

@Component({
  selector: 'app-invoice-filter-component',
  imports: [FormsModule],
  templateUrl: './invoice-filter-component.html',
  styleUrl: './invoice-filter-component.scss',
})
export class InvoiceFilterComponent implements OnInit {
  @Input() limitSelection: number[] = [];
  @Input() limit: number = 10;

  @Input() showCostCentreFilter: boolean = false;
  @Input() showUserFilter: boolean = false;

  @Output() updateFilter = new EventEmitter<GetPaymentRequestsByUserOptions>();
  @Output() limitChange = new EventEmitter<number>();

  teams: TeamDto[] = [];
  costCentres: CostCentreDto[] = [];
  users: UserDto[] = [];

  filterInvoiceNumber: string = '';
  filterStatus: TransactionStatus | undefined = undefined;
  filterMinCreatedAt: string = '';
  filterMaxCreatedAt: string = '';
  filterMinPaidAt: string = '';
  filterMaxPaidAt: string = '';
  filterMinAmount: string = '';
  filterMaxAmount: string = '';
  filterPurpose: string = '';
  filterTeamId: number | undefined = undefined;
  filterPayoutType: PayoutType | undefined = undefined;
  filterCostCentreId: number | undefined = undefined;
  filterUserId: number | undefined = undefined;

  private readonly filterInvoiceNumberSubject = new Subject<string>();
  private readonly filterPurposeSubject = new Subject<string>();
  private readonly filterMinAmountSubject = new Subject<string>();
  private readonly filterMaxAmountSubject = new Subject<string>();
  private readonly filterMinDateSubject = new Subject<string>();
  private readonly filterMaxDateSubject = new Subject<string>();
  private readonly filterMinPaidAtSubject = new Subject<string>();
  private readonly filterMaxPaidAtSubject = new Subject<string>();
  private readonly filterStatusSubject = new Subject<TransactionStatus | undefined>();
  private readonly filterTeamSubject = new Subject<number | undefined>();
  private readonly filterPayoutTypeSubject = new Subject<PayoutType | undefined>();
  private readonly filterCostCentreSubject = new Subject<number | undefined>();
  private readonly filterUserSubject = new Subject<number | undefined>();

  TransactionStatus = TransactionStatus;
  TransactionStatusLabels = TransactionStatusLabels;
  transactionStatusOptions = Object.values(TransactionStatus).filter(
    (v) => typeof v === 'number',
  ) as TransactionStatus[];

  PayoutType = PayoutType;
  PayoutTypeLabels = PayoutTypeLabels;
  payoutTypeOptions = Object.values(PayoutType).filter(
    (v) => typeof v === 'number',
  ) as PayoutType[];

  constructor(
    private readonly teamService: TeamService,
    private readonly costCentreService: CostCentreService,
    private readonly userService: UserService,
  ) {}

  ngOnInit(): void {
    this.filterMinCreatedAt = new Date(0).toISOString().split('T')[0];
    this.filterMaxCreatedAt = new Date().toISOString().split('T')[0];
    this.filterMinPaidAt = new Date(0).toISOString().split('T')[0];
    this.filterMaxPaidAt = new Date().toISOString().split('T')[0];

    this.teamService.getTeams({ Limit: 1000 }).subscribe({
      next: (data) => {
        this.teams = data?.items ?? [];
      },
      error: () => {},
    });

    if (this.showCostCentreFilter) {
      this.costCentreService.getCostCentres({ Limit: 1000 }).subscribe({
        next: (data) => {
          this.costCentres = data?.items ?? [];
        },
        error: () => {},
      });
    }

    if (this.showUserFilter) {
      this.userService.getUser({ Limit: 1000 }).subscribe({
        next: (data) => {
          this.users = data?.items ?? [];
        },
        error: () => {},
      });
    }

    this.filterCostCentreSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterCostCentreId = value;
      this.emitFilter();
    });

    this.filterUserSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterUserId = value;
      this.emitFilter();
    });

    this.filterInvoiceNumberSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterInvoiceNumber = value;
      this.emitFilter();
    });

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

    this.filterMinDateSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMinCreatedAt = value;
      this.emitFilter();
    });

    this.filterMaxDateSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMaxCreatedAt = value;
      this.emitFilter();
    });

    this.filterMinPaidAtSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMinPaidAt = value;
      this.emitFilter();
    });

    this.filterMaxPaidAtSubject.pipe(debounceTime(400)).subscribe((value) => {
      this.filterMaxPaidAt = value;
      this.emitFilter();
    });

    this.filterStatusSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterStatus = value;
      this.emitFilter();
    });

    this.filterTeamSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterTeamId = value;
      this.emitFilter();
    });

    this.filterPayoutTypeSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterPayoutType = value;
      this.emitFilter();
    });
  }

  emitFilter(): void {
    this.updateFilter.emit(this.getFilterOptions());
  }

  getFilterOptions(): GetPaymentRequestsByUserOptions {
    return {
      InvoiceNumber: this.filterInvoiceNumber || undefined,
      Status: this.filterStatus,
      MinCreatedAt: this.filterMinCreatedAt || undefined,
      MaxCreatedAt: this.filterMaxCreatedAt || undefined,
      MinPaidAt: this.filterMinPaidAt || undefined,
      MaxPaidAt: this.filterMaxPaidAt || undefined,
      MinAmount: this.filterMinAmount ? Number(this.filterMinAmount) : undefined,
      MaxAmount: this.filterMaxAmount ? Number(this.filterMaxAmount) : undefined,
      PurposeOfPayment: this.filterPurpose || undefined,
      TeamId: this.filterTeamId,
      PayoutType: this.filterPayoutType,
      CostCentreId: this.filterCostCentreId,
      UserId: this.filterUserId,
      Limit: undefined,
      Offset: undefined,
    };
  }

  onInvoiceNumberChange(event: Event): void {
    this.filterInvoiceNumberSubject.next((event.target as HTMLInputElement).value);
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

  onMinDateChange(event: Event): void {
    this.filterMinDateSubject.next((event.target as HTMLInputElement).value);
  }

  onMaxDateChange(event: Event): void {
    this.filterMaxDateSubject.next((event.target as HTMLInputElement).value);
  }

  onMinPaidAtChange(event: Event): void {
    this.filterMinPaidAtSubject.next((event.target as HTMLInputElement).value);
  }

  onMaxPaidAtChange(event: Event): void {
    this.filterMaxPaidAtSubject.next((event.target as HTMLInputElement).value);
  }

  onStatusChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterStatusSubject.next(value !== '' ? Number(value) : undefined);
  }

  onTeamChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterTeamSubject.next(value !== '' ? Number(value) : undefined);
  }

  onPayoutTypeChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterPayoutTypeSubject.next(value !== '' ? Number(value) : undefined);
  }

  onCostCentreChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterCostCentreSubject.next(value !== '' ? Number(value) : undefined);
  }

  onUserChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterUserSubject.next(value !== '' ? Number(value) : undefined);
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
