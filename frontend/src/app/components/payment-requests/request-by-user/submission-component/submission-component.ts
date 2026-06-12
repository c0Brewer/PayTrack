import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnDestroy, OnInit, inject } from '@angular/core';
import {
  AbstractControl,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, take, takeUntil } from 'rxjs';

import { DisableOfflineActionDirective } from '../../../../directives/disable-offline-action.directive';
import { AuthService } from '../../../../services/auth/auth-service';
import { BankAccountService } from '../../../../services/bank-account/bank-account-service';
import { NotificationService } from '../../../../services/notification/notification-service';
import {
  OfflineInvoiceSubmissionDraft,
  OfflineInvoiceSubmissionItem,
  OfflineInvoiceSubmissionQueueService,
} from '../../../../services/offline/offline-invoice-submission-queue.service';
import { OfflineService } from '../../../../services/offline/offline-service';
import { PaymentRequestByUserService } from '../../../../services/payment-request-by-user/payment-request-by-user-service';
import { TeamService } from '../../../../services/team/team-service';
import {
  DuplicatePaymentRequestByUserDto,
  TeamDto,
  CreatePaymentRequestByUserDto,
  PayoutType,
  BankAccount,
} from '../../../../types/exporter';
import { BoxComponent } from '../../../general/boxes/box-component/box-component';
import { DuplicateListModalComponent } from '../duplicate-list-modal-component/duplicate-list-modal-component';

function maxDateValidator(maxDate: Date): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const selected = new Date(control.value);
    const latestAllowed = new Date(maxDate.getFullYear(), maxDate.getMonth(), maxDate.getDate());

    return selected > latestAllowed
      ? { maxDate: { max: latestAllowed.toISOString().slice(0, 10) } }
      : null;
  };
}

@Component({
  selector: 'app-receipt-submit-component',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    BoxComponent,
    DuplicateListModalComponent,
    DisableOfflineActionDirective,
  ],
  templateUrl: './submission-component.html',
  styleUrl: './submission-component.scss',
})
export class ReceiptSubmitComponent implements OnInit, OnDestroy {
  protected readonly offlineService = inject(OfflineService);
  protected readonly offlineInvoiceSubmissionQueueService = inject(
    OfflineInvoiceSubmissionQueueService,
  );

  form!: FormGroup;
  teams: TeamDto[] = [];
  bankAccounts: BankAccount[] = [];
  isSubmitting = false;
  selectedFile: File | null = null;
  selectedFileName = '';
  maxInvoiceDate = new Date().toISOString().split('T')[0];
  duplicateCandidates: DuplicatePaymentRequestByUserDto[] = [];
  isDuplicateModalOpen = false;
  pendingSubmissionPayload: CreatePaymentRequestByUserDto | null = null;
  pendingSubmissionFile: File | null = null;

  readonly PayoutType = PayoutType;

  private readonly destroy$ = new Subject<void>();

  payoutTypeOptions = Object.values(PayoutType).filter(
    (v) => typeof v === 'number',
  ) as PayoutType[];

  payoutTypeLabels: Record<PayoutType, string> = {
    [PayoutType.User]: 'Already paid by user (you)',
    [PayoutType.NotYetPaid]: 'Not yet paid by user. Should be paid to invoice-issuer.',
    [PayoutType.AlreadyPaid]: 'Already paid (documentation only)',
  };

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly paymentRequestByUserService: PaymentRequestByUserService,
    private readonly teamService: TeamService,
    private readonly bankAccountService: BankAccountService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
    private readonly changeDetectorRef: ChangeDetectorRef,
    private readonly ngZone: NgZone,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadTeams();
    this.loadBankAccounts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      invoiceNumber: ['', [Validators.required, Validators.maxLength(100)]],
      comment: ['', [Validators.maxLength(500)]],
      payoutType: [null, Validators.required],
      bankAccountId: [null, [Validators.required, Validators.min(1)]],
      creditorName: [null],
      teamId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      purposeOfPayment: ['', [Validators.required, Validators.maxLength(255)]],
      paidAt: ['', [Validators.required, maxDateValidator(new Date())]],
      receipt: [null, Validators.required],
    });

    this.form
      .get('payoutType')!
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe((value) => {
        const bankCtrl = this.form.get('bankAccountId');
        const creditorCtrl = this.form.get('creditorName');

        if (value === PayoutType.User) {
          bankCtrl?.setValidators([Validators.required, Validators.min(1)]);
        } else {
          bankCtrl?.clearValidators();
          bankCtrl?.setValue(null);
        }
        bankCtrl?.updateValueAndValidity();

        if (value === PayoutType.NotYetPaid) {
          creditorCtrl?.setValidators([Validators.required, Validators.maxLength(255)]);
        } else {
          creditorCtrl?.clearValidators();
          creditorCtrl?.setValue(null);
        }
        creditorCtrl?.updateValueAndValidity();
      });
  }

  private loadTeams(): void {
    this.teamService
      .getTeams({ IsActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (teams) => {
          if (teams.items != null) {
            this.teams = teams.items;
          }

          this.authService.currentUser$.pipe(take(1)).subscribe((user) => {
            const userTeamId = user?.team?.id;
            if (userTeamId != null) {
              this.form.get('teamId')!.setValue(userTeamId);
            }
          });

          this.changeDetectorRef.markForCheck();
        },
        error: () => this.notificationService.showError('Failed to load teams.'),
      });
  }

  private loadBankAccounts(): void {
    this.bankAccountService
      .getBankAccounts()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (bankAccounts) => {
          this.bankAccounts = bankAccounts;

          this.changeDetectorRef.markForCheck();
        },
        error: () => this.notificationService.showError('Failed to load teams.'),
      });
  }

  getShortenedIban(iban: string): string {
    if (!iban) return '';

    const cleaned = iban.replace(/\s+/g, '');

    if (cleaned.length <= 8) return cleaned;

    const start = cleaned.slice(0, 4);
    const end = cleaned.slice(-4);

    return `${start} **** **** ${end}`;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.setReceiptFile(file);
  }

  onFileDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onFileDropped(event: DragEvent): void {
    event.preventDefault();

    const file = event.dataTransfer?.files?.[0];
    if (!file) return;

    this.setReceiptFile(file);
  }

  onManageBankAccountClick(event: Event): void {
    event.preventDefault();

    this.router.navigate(['/settings'], { fragment: 'bank-accounts' });
  }

  private setReceiptFile(file: File): void {
    const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png'];
    const maxSizeMb = 20;
    const receiptControl = this.form.get('receipt')!;

    receiptControl.markAsTouched();

    if (!allowedTypes.includes(file.type)) {
      this.selectedFile = null;
      this.selectedFileName = '';
      receiptControl.setErrors({ invalidType: true });
      return;
    }
    if (file.size > maxSizeMb * 1024 * 1024) {
      this.selectedFile = null;
      this.selectedFileName = '';
      receiptControl.setErrors({ tooLarge: true });
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    receiptControl.setValue(file.name);
    receiptControl.setErrors(null);
  }

  getError(field: string): string | null {
    const control = this.form.get(field);
    if (!control || !control.invalid || !control.touched) return null;

    const errors = control.errors!;
    if (errors['required']) return 'This field is required.';
    if (errors['min']) return `Minimum value is ${errors['min'].min}.`;
    if (errors['maxDate']) return 'Invoice date cannot be in the future.';
    if (errors['maxlength'])
      return `Maximum length is ${errors['maxlength'].requiredLength} characters.`;
    if (errors['invalidType']) return 'Only PDF, JPG, or PNG files are allowed.';
    if (errors['tooLarge']) return 'File must be smaller than 20 MB.';
    return 'Invalid value.';
  }

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && control.touched;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || !this.selectedFile) return;

    this.isSubmitting = true;
    const v = this.form.value;
    const payoutType = this.toPayoutType(v.payoutType);

    if (payoutType === null) {
      this.notificationService.showError('Invalid payout type.');
      this.isSubmitting = false;
      return;
    }

    const payload: CreatePaymentRequestByUserDto = {
      invoiceNumber: v.invoiceNumber,
      comment: v.comment,
      payoutType: payoutType,
      bankAccountId: payoutType === PayoutType.User ? Number(v.bankAccountId) : null,
      creditorName: payoutType === PayoutType.NotYetPaid ? v.creditorName : null,
      receipt: '', // ignored — real file is passed separately below
      transaction: {
        teamId: Number(v.teamId),
        amount: Number(v.amount),
        purposeOfPayment: v.purposeOfPayment,
        paidAt: new Date(v.paidAt).toISOString(),
      },
    };

    if (this.offlineService.isOffline()) {
      void this.queueOfflineSubmission(payload, this.selectedFile);
      return;
    }

    this.paymentRequestByUserService
      .getDuplicatePaymentRequestsByUser({
        TeamId: payload.transaction.teamId,
        Amount: payload.transaction.amount,
        PaidAt: payload.transaction.paidAt,
        InvoiceNumber: payload.invoiceNumber,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (duplicates) => {
          this.ngZone.run(() => {
            if (duplicates.length > 0) {
              this.duplicateCandidates = duplicates;
              this.pendingSubmissionPayload = payload;
              this.pendingSubmissionFile = this.selectedFile;
              this.isDuplicateModalOpen = true;
              this.isSubmitting = false;
              this.changeDetectorRef.detectChanges();
              return;
            }

            this.submitPaymentRequest(payload, this.selectedFile!);
          });
        },
        error: (err: Error) => {
          this.ngZone.run(() => {
            this.notificationService.showError(err.message ?? 'Duplicate check failed.');
            this.isSubmitting = false;
            this.changeDetectorRef.detectChanges();
          });
        },
      });
  }

  onDuplicateModalCancel(): void {
    this.isDuplicateModalOpen = false;
    this.duplicateCandidates = [];
    this.pendingSubmissionPayload = null;
    this.pendingSubmissionFile = null;
  }

  onDuplicateModalSubmitRegardless(): void {
    if (!this.pendingSubmissionPayload || !this.pendingSubmissionFile) {
      this.notificationService.showError('Submission data was lost. Please submit again.');
      this.onDuplicateModalCancel();
      this.isSubmitting = false;
      return;
    }

    const payload = this.pendingSubmissionPayload;
    const file = this.pendingSubmissionFile;

    this.isDuplicateModalOpen = false;
    this.duplicateCandidates = [];
    this.pendingSubmissionPayload = null;
    this.pendingSubmissionFile = null;
    this.isSubmitting = true;

    this.submitPaymentRequest(payload, file);
  }

  private submitPaymentRequest(payload: CreatePaymentRequestByUserDto, file: File): void {
    this.paymentRequestByUserService
      .createPaymentRequestByUser(payload, file)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Invoice submitted successfully.');
          this.resetSubmissionState();
          this.router.navigate(['/']);
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Submission failed.');
          this.isSubmitting = false;
          this.changeDetectorRef.detectChanges();
        },
      });
  }

  async loadPendingSubmission(item: OfflineInvoiceSubmissionItem): Promise<void> {
    const draft = await this.offlineInvoiceSubmissionQueueService.getSubmissionDraft(item.id);
    if (!draft) {
      this.notificationService.showError('The offline invoice draft could not be loaded.');
      return;
    }

    this.applyOfflineDraftToForm(draft);
    await this.offlineInvoiceSubmissionQueueService.removeSubmission(item.id);
    this.notificationService.showSuccess('Offline invoice draft loaded into the form.');
  }

  async removePendingSubmission(item: OfflineInvoiceSubmissionItem): Promise<void> {
    await this.offlineInvoiceSubmissionQueueService.removeSubmission(item.id);
    this.notificationService.showSuccess('Pending offline invoice removed.');
  }

  getPendingStatusLabel(status: OfflineInvoiceSubmissionItem['status']): string {
    return status === 'pending' ? 'Stored Offline' : 'Stored Offline';
  }

  getPendingStatusClass(status: OfflineInvoiceSubmissionItem['status']): string {
    return status === 'pending'
      ? 'offline-queue__badge offline-queue__badge--pending'
      : 'offline-queue__badge offline-queue__badge--pending';
  }

  getTeamName(teamId: number): string {
    return this.teams.find((team) => team.id === teamId)?.name ?? `Team #${teamId}`;
  }

  private async queueOfflineSubmission(
    payload: CreatePaymentRequestByUserDto,
    file: File,
  ): Promise<void> {
    try {
      await this.offlineInvoiceSubmissionQueueService.queueSubmission(payload, file);
      this.notificationService.showSuccess(
        'Invoice saved locally. It will be synchronized once the connection is restored.',
        5000,
      );
      this.resetSubmissionState();
    } catch (error) {
      this.notificationService.showError(
        error instanceof Error ? error.message : 'Could not save invoice offline.',
      );
      this.isSubmitting = false;
      this.changeDetectorRef.detectChanges();
    }
  }

  private resetSubmissionState(): void {
    this.form.reset();
    this.authService.currentUser$.pipe(take(1)).subscribe((user) => {
      const userTeamId = user?.team?.id;
      if (userTeamId != null) {
        this.form.get('teamId')?.setValue(userTeamId);
      }
    });
    this.selectedFile = null;
    this.selectedFileName = '';
    this.duplicateCandidates = [];
    this.isDuplicateModalOpen = false;
    this.pendingSubmissionPayload = null;
    this.pendingSubmissionFile = null;
    this.isSubmitting = false;
    this.changeDetectorRef.detectChanges();
  }

  private applyOfflineDraftToForm(draft: OfflineInvoiceSubmissionDraft): void {
    const paidAt = draft.payload.transaction.paidAt
      ? new Date(draft.payload.transaction.paidAt).toISOString().slice(0, 10)
      : '';

    this.form.patchValue({
      invoiceNumber: draft.payload.invoiceNumber,
      comment: draft.payload.comment ?? '',
      payoutType: draft.payload.payoutType,
      bankAccountId: draft.payload.bankAccountId,
      creditorName: draft.payload.creditorName,
      teamId: draft.payload.transaction.teamId,
      amount: draft.payload.transaction.amount,
      purposeOfPayment: draft.payload.transaction.purposeOfPayment,
      paidAt,
      receipt: draft.file.name,
    });

    this.selectedFile = draft.file;
    this.selectedFileName = draft.file.name;
    this.form.get('receipt')?.setErrors(null);
    this.form.markAsPristine();
    this.changeDetectorRef.detectChanges();
  }

  toPayoutType(value: unknown): PayoutType | null {
    const num = Number(value);

    return Object.values(PayoutType).includes(num) ? (num as PayoutType) : null;
  }

  getDuplicateUserName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return (
      (
        duplicate.paymentRequestByUser.user as
          | {
              name?: string | null;
            }
          | null
          | undefined
      )?.name ?? 'Unknown user'
    );
  }

  getDuplicateTeamName(duplicate: DuplicatePaymentRequestByUserDto): string {
    return (
      (
        duplicate.paymentRequestByUser.team as
          | {
              name?: string | null;
            }
          | null
          | undefined
      )?.name ?? 'Unknown team'
    );
  }

  protected readonly event = event;
}
