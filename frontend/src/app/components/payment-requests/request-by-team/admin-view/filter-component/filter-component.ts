import { Component, EventEmitter, OnInit, Output, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, Subject } from 'rxjs';

import { TeamService } from '../../../../../services/team/team-service';
import { UserService } from '../../../../../services/user/user-service';
import {
  GetPaymentRequestsByTeamOptions,
  TEAM_REQUEST_ALLOWED_STATUSES,
  TeamDto,
  TransactionStatus,
  TransactionStatusLabels,
  UserDto,
} from '../../../../../types/exporter';

@Component({
  selector: 'app-team-request-admin-filter-component',
  imports: [FormsModule],
  templateUrl: './filter-component.html',
  styleUrl: './filter-component.scss',
})
export class TeamRequestAdminFilterComponent implements OnInit {
  @Input() limitSelection: number[] = [];
  @Input() limit: number = 10;

  @Output() updateFilter = new EventEmitter<GetPaymentRequestsByTeamOptions>();
  @Output() limitChange = new EventEmitter<number>();

  teams: TeamDto[] = [];
  users: UserDto[] = [];

  filterPurpose: string = '';
  filterMinAmount: string = '';
  filterMaxAmount: string = '';
  filterMinDueDate: string = '';
  filterMaxDueDate: string = '';
  filterStatus: TransactionStatus | undefined = undefined;
  filterTeamId: number | undefined = undefined;
  filterUserId: number | undefined = undefined;

  private readonly filterPurposeSubject = new Subject<string>();
  private readonly filterMinAmountSubject = new Subject<string>();
  private readonly filterMaxAmountSubject = new Subject<string>();
  private readonly filterMinDueDateSubject = new Subject<string>();
  private readonly filterMaxDueDateSubject = new Subject<string>();
  private readonly filterStatusSubject = new Subject<TransactionStatus | undefined>();
  private readonly filterTeamSubject = new Subject<number | undefined>();
  private readonly filterUserSubject = new Subject<number | undefined>();

  TransactionStatusLabels = TransactionStatusLabels;
  transactionStatusOptions: TransactionStatus[] = [...TEAM_REQUEST_ALLOWED_STATUSES];

  constructor(
    private readonly teamService: TeamService,
    private readonly userService: UserService,
  ) {}

  ngOnInit(): void {
    this.teamService.getTeams({ Limit: 1000 }).subscribe({
      next: (data) => {
        this.teams = data?.items ?? [];
      },
      error: () => {},
    });

    this.userService.getUser({ Limit: 1000 }).subscribe({
      next: (data) => {
        this.users = data?.items ?? [];
      },
      error: () => {},
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

    this.filterTeamSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterTeamId = value;
      this.emitFilter();
    });

    this.filterUserSubject.pipe(debounceTime(100)).subscribe((value) => {
      this.filterUserId = value;
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
      TeamId: this.filterTeamId,
      UserId: this.filterUserId,
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

  onTeamChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterTeamSubject.next(value !== '' ? Number(value) : undefined);
  }

  onUserChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterUserSubject.next(value !== '' ? Number(value) : undefined);
  }

  onLimitChange(): void {
    this.limitChange.emit(this.limit);
  }
}
