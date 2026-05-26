import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Subject, map, takeUntil } from 'rxjs';

import { CostCentreService } from '../../../../services/cost-centre/cost-centre-service';
import { NotificationService } from '../../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../../services/payment-request-by-team/payment-request-by-team-service';
import { TeamService } from '../../../../services/team/team-service';
import { UserService } from '../../../../services/user/user-service';
import { CostCentreDto, CreatePaymentRequestByTeamDto, TeamDto } from '../../../../types/exporter';
import { BoxComponent } from '../../../general/boxes/box-component/box-component';
import {
  TypeaheadItem,
  TypeaheadSelectComponent,
} from '../../../general/typeahead-select-component/typeahead-select-component';
import { CsvBulkImportModalComponent } from '../csv-bulk-import-modal/csv-bulk-import-modal';

function minDateValidator(min: Date): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const selected = new Date(control.value);
    const minMidnight = new Date(min.getFullYear(), min.getMonth(), min.getDate());
    return selected < minMidnight ? { minDate: { min: min.toISOString().slice(0, 10) } } : null;
  };
}

@Component({
  selector: 'app-payment-request-by-team-component',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    BoxComponent,
    TypeaheadSelectComponent,
    CsvBulkImportModalComponent,
  ],
  templateUrl: './submission-component.html',
  styleUrl: './submission-component.scss',
})
export class PaymentRequestByTeamComponent implements OnInit, OnDestroy {
  @ViewChild(TypeaheadSelectComponent) private readonly typeaheadRef!: TypeaheadSelectComponent;
  @ViewChild('csvFileInput') private readonly csvFileInputRef!: ElementRef<HTMLInputElement>;

  form!: FormGroup;
  teams: TeamDto[] = [];
  costCentres: CostCentreDto[] = [];
  allUsers: TypeaheadItem[] = [];
  isSubmitting = false;

  isCsvModalOpen = false;
  csvImportFile: File | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly paymentRequestByTeamService: PaymentRequestByTeamService,
    private readonly teamService: TeamService,
    private readonly costCentreService: CostCentreService,
    private readonly userService: UserService,
    private readonly notificationService: NotificationService,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadTeams();
    this.loadCostCentres();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      userId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      purposeOfPayment: ['', [Validators.required, Validators.maxLength(255)]],
      teamId: [null, Validators.required],
      costCentreId: [null, Validators.required],
      dueDate: ['', [Validators.required, minDateValidator(new Date())]],
    });
  }

  private loadTeams(): void {
    this.teamService
      .getTeams({})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          if (result.items != null) this.teams = result.items;
        },
        error: () => this.notificationService.showError('Failed to load teams.'),
      });
  }

  private loadUsers(): void {
    this.userService
      .getUser({ Limit: 1000 })
      .pipe(
        takeUntil(this.destroy$),
        map((r) =>
          (r.items ?? []).map((u) => ({
            id: u.id!,
            primaryText: u.name ?? '',
            secondaryText: u.email ?? undefined,
          })),
        ),
      )
      .subscribe({
        next: (users) => (this.allUsers = users),
        error: () => this.notificationService.showError('Failed to load users.'),
      });
  }

  private loadCostCentres(): void {
    this.costCentreService
      .getCostCentres({})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          if (result.items != null) this.costCentres = result.items;
        },
        error: () => this.notificationService.showError('Failed to load cost centres.'),
      });
  }

  onOpenCsvPicker(): void {
    this.csvFileInputRef.nativeElement.click();
  }

  onCsvFileInputChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    if (!file) return;
    this.csvImportFile = file;
    this.isCsvModalOpen = true;
    (event.target as HTMLInputElement).value = '';
  }

  onCsvModalClose(): void {
    this.isCsvModalOpen = false;
    this.csvImportFile = null;
  }

  onUserSelected(item: TypeaheadItem): void {
    this.form.get('userId')!.setValue(item.id);
  }

  onUserCleared(): void {
    this.form.get('userId')!.setValue(null);
  }

  getError(field: string): string | null {
    const control = this.form.get(field);
    if (!control || !control.invalid || !control.touched) return null;

    const errors = control.errors!;
    if (errors['required']) return 'This field is required.';
    if (errors['min']) return `Minimum value is ${errors['min'].min}.`;
    if (errors['maxlength'])
      return `Maximum length is ${errors['maxlength'].requiredLength} characters.`;
    if (errors['minDate']) return `Due date must be today or in the future.`;
    return 'Invalid value.';
  }

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && control.touched;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.isSubmitting = true;
    const v = this.form.value;

    const payload: CreatePaymentRequestByTeamDto = {
      transaction: {
        teamId: Number(v.teamId),
        amount: Number(v.amount),
        purposeOfPayment: v.purposeOfPayment,
        paidAt: new Date(0).toISOString(),
      },
      userToAssignToId: Number(v.userId),
      dueDate: new Date(v.dueDate).toISOString(),
      costCentreId: Number(v.costCentreId),
    };

    this.paymentRequestByTeamService
      .createPaymentRequestByTeam(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Payment request created.');
          this.form.reset();
          this.typeaheadRef.reset();
          this.isSubmitting = false;
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Submission failed.');
          this.isSubmitting = false;
        },
      });
  }
}
