import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Subject, catchError, debounceTime, distinctUntilChanged, filter, of, switchMap, takeUntil } from 'rxjs';

import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { CostCentreService } from '../../../services/cost-centre/cost-centre-service';
import { TeamService } from '../../../services/team/team-service';
import { UserService } from '../../../services/user/user-service';
import {
  CostCentreDto,
  CreatePaymentRequestByTeamDto,
  TeamDto,
  UserDto,
} from '../../../types/exporter';
import { BoxComponent } from '../../general/boxes/box-component/box-component';

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
  imports: [CommonModule, ReactiveFormsModule, BoxComponent],
  templateUrl: './payment-request-by-team-component.html',
  styleUrl: './payment-request-by-team-component.scss',
})
export class PaymentRequestByTeamComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  teams: TeamDto[] = [];
  costCentres: CostCentreDto[] = [];
  userSearchResults: UserDto[] = [];
  showUserDropdown = false;
  selectedUser: UserDto | null = null;
  isSubmitting = false;

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
    this.setupUserSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      userSearch: [''],
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

  private setupUserSearch(): void {
    this.form
      .get('userSearch')!
      .valueChanges.pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged(),
        filter((term) => typeof term === 'string' && term.trim().length >= 2),
        switchMap((term) =>
          this.userService
            .getUser({ Name: term, Limit: 8 })
            .pipe(catchError(() => of(null))),
        ),
      )
      .subscribe((result) => {
        this.userSearchResults = result?.items ?? [];
        this.showUserDropdown = this.userSearchResults.length > 0;
      });
  }

  selectUser(user: UserDto): void {
    this.selectedUser = user;
    this.form.get('userId')!.setValue(user.id);
    this.form.get('userSearch')!.setValue(`${user.name} (${user.email})`, { emitEvent: false });
    this.showUserDropdown = false;
  }

  clearUserSelection(): void {
    this.selectedUser = null;
    this.form.get('userId')!.setValue(null);
    this.form.get('userSearch')!.setValue('');
    this.userSearchResults = [];
    this.showUserDropdown = false;
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
          this.selectedUser = null;
          this.userSearchResults = [];
          this.showUserDropdown = false;
          this.isSubmitting = false;
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Submission failed.');
          this.isSubmitting = false;
        },
      });
  }
}
