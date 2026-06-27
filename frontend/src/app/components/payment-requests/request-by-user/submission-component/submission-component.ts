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
import { ActivatedRoute, Router } from '@angular/router';
import { startWith, Subject, take, takeUntil } from 'rxjs';

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
import { PaymentRequestStatusRefreshService } from '../../../../services/payment-request-by-user/payment-request-status-refresh-service';
import { TeamService } from '../../../../services/team/team-service';
import {
  DuplicatePaymentRequestByUserDto,
  TeamDto,
  CreatePaymentRequestByUserDto,
  PayoutType,
  BankAccount,
  PaymentRequestByUserDto,
  TransactionStatus,
  ReceiptExtractionDto,
} from '../../../../types/exporter';
import { BoxComponent } from '../../../general/boxes/box-component/box-component';
import {
  type DuplicateInvoiceSummary,
  DuplicateListModalComponent,
} from '../duplicate-list-modal-component/duplicate-list-modal-component';

function maxDateValidator(maxDate: Date): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const selected = new Date(`${control.value}T00:00:00`);
    const latestAllowed = startOfLocalDay(maxDate);

    return selected > latestAllowed
      ? { maxDate: { max: toLocalDateInputValue(latestAllowed) } }
      : null;
  };
}

function startOfLocalDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function toLocalDateInputValue(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

@Component({
  selector: 'app-receipt-submit-component',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BoxComponent, DuplicateListModalComponent],
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
  maxInvoiceDate = toLocalDateInputValue(new Date());
  duplicateCandidates: DuplicatePaymentRequestByUserDto[] = [];
  duplicateSourceInvoice: DuplicateInvoiceSummary | null = null;
  isDuplicateModalOpen = false;
  pendingSubmissionPayload: CreatePaymentRequestByUserDto | null = null;
  pendingSubmissionFile: File | null = null;
  isEditMode = false;
  editingInvoiceId: number | null = null;
  changeRequestMessage: string | null = null;
  isExtractingReceiptData = false;
  receiptExtractionMessage = '';
  receiptExtractionStatus: 'idle' | 'loading' | 'success' | 'partial' | 'error' = 'idle';
  receiptExtractionResult: ReceiptExtractionDto | null = null;
  currentUserName = 'Current user';

  readonly PayoutType = PayoutType;

  private readonly destroy$ = new Subject<void>();
  private receiptExtractionRequestId = 0;

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
    private readonly statusRefreshService: PaymentRequestStatusRefreshService,
    private readonly teamService: TeamService,
    private readonly bankAccountService: BankAccountService,
    private readonly notificationService: NotificationService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly changeDetectorRef: ChangeDetectorRef,
    private readonly ngZone: NgZone,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadCurrentUserName();
    this.loadTeams();
    this.loadBankAccounts();
    this.loadInvoiceForEditing();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      invoiceNumber: ['', [Validators.required, Validators.maxLength(100)]],
      comment: ['', [Validators.maxLength(500)]],
      payoutType: [this.getPayoutTypeControlValue(PayoutType.User), Validators.required],
      bankAccountId: [null, [Validators.required, Validators.min(1)]],
      creditorName: [null],
      dueDate: [null],
      teamId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      purposeOfPayment: ['', [Validators.required, Validators.maxLength(255)]],
      paidAt: ['', [Validators.required, maxDateValidator(new Date())]],
      receipt: [null, Validators.required],
    });

    this.form
      .get('payoutType')!
      .valueChanges.pipe(
        startWith(this.form.get('payoutType')?.value),
        takeUntil(this.destroy$),
      )
      .subscribe((value) => {
        const payoutType = this.toPayoutType(value);
        const bankCtrl = this.form.get('bankAccountId');
        const creditorCtrl = this.form.get('creditorName');
        const dueDateCtrl = this.form.get('dueDate');

        if (payoutType === PayoutType.User) {
          bankCtrl?.setValidators([Validators.required, Validators.min(1)]);
        } else {
          bankCtrl?.clearValidators();
        }
        bankCtrl?.updateValueAndValidity();

        if (payoutType === PayoutType.NotYetPaid) {
          creditorCtrl?.setValidators([Validators.required, Validators.maxLength(255)]);
          dueDateCtrl?.setValidators([Validators.required]);
        } else {
          creditorCtrl?.clearValidators();
          dueDateCtrl?.clearValidators();
        }
        creditorCtrl?.updateValueAndValidity();
        dueDateCtrl?.updateValueAndValidity();
      });
  }

  private loadInvoiceForEditing(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;

    this.isEditMode = true;
    this.editingInvoiceId = id;
    this.form.get('receipt')?.clearValidators();
    this.form.get('receipt')?.updateValueAndValidity();

    this.paymentRequestByUserService
      .getPaymentRequestsByUserById(id, {
        IncludeTeam: true,
        IncludeBankAccount: true,
        IncludeStatusHistory: true,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invoice) => {
          if (invoice.status !== TransactionStatus.ChangesRequested) {
            this.notificationService.showError(
              'Only invoices with requested changes can be edited.',
            );
            this.router.navigate(['/my-invoices', id]);
            return;
          }

          this.patchInvoice(invoice);
          this.changeDetectorRef.markForCheck();
        },
        error: (err: Error) => {
          this.notificationService.showError('Could not load invoice: ' + err.message);
          this.router.navigate(['/my-invoices']);
        },
      });
  }

  private patchInvoice(invoice: PaymentRequestByUserDto): void {
    this.form.patchValue({
      invoiceNumber: invoice.invoiceNumber,
      comment: invoice.comment ?? '',
      payoutType: this.getPayoutTypeControlValue(invoice.payoutType),
      bankAccountId: invoice.bankAccount?.id ?? null,
      creditorName: invoice.creditorName ?? '',
      dueDate: invoice.dueDate?.slice(0, 10) ?? null,
      teamId: invoice.team?.id ?? null,
      amount: invoice.amount,
      purposeOfPayment: invoice.purposeOfPayment,
      paidAt: invoice.paidAt?.slice(0, 10) ?? '',
    });

    this.selectedFile = null;
    this.selectedFileName = 'Current receipt will be kept';
    const receiptControl = this.form.get('receipt');
    receiptControl?.setValue('existing-receipt');
    receiptControl?.setErrors(null);
    receiptControl?.markAsPristine();

    this.changeRequestMessage =
      [...(invoice.statusHistory ?? [])]
        .filter(
          (entry) =>
            entry.toStatus === TransactionStatus.ChangesRequested && !!entry.comment?.trim(),
        )
        .sort(
          (left, right) => new Date(right.changedAt).getTime() - new Date(left.changedAt).getTime(),
        )[0]
        ?.comment?.trim() ?? null;
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

  private loadCurrentUserName(): void {
    this.authService.currentUser$.pipe(takeUntil(this.destroy$)).subscribe((user) => {
      this.currentUserName = user?.name ?? 'Current user';
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
      this.clearReceiptExtractionState();
      return;
    }
    if (file.size > maxSizeMb * 1024 * 1024) {
      this.selectedFile = null;
      this.selectedFileName = '';
      receiptControl.setErrors({ tooLarge: true });
      this.clearReceiptExtractionState();
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    receiptControl.setValue(file.name);
    receiptControl.setErrors(null);
    this.extractReceiptData(file);
  }

  private extractReceiptData(file: File): void {
    const requestId = ++this.receiptExtractionRequestId;
    this.receiptExtractionResult = null;

    if (this.offlineService.isOffline()) {
      this.isExtractingReceiptData = false;
      this.receiptExtractionStatus = 'partial';
      this.receiptExtractionMessage =
        'Automatic field detection is unavailable while you are offline.';
      return;
    }

    this.isExtractingReceiptData = true;
    this.receiptExtractionStatus = 'loading';
    this.receiptExtractionMessage = 'Looking for invoice details in the uploaded receipt...';

    try {
      this.paymentRequestByUserService
        .extractReceiptData(file)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (result) => {
            if (requestId !== this.receiptExtractionRequestId) {
              return;
            }

            const appliedFields = this.applyReceiptExtractionSuggestions(result);
            this.receiptExtractionResult = result;
            this.isExtractingReceiptData = false;
            this.receiptExtractionStatus = appliedFields > 0 ? 'success' : 'partial';
            this.receiptExtractionMessage = this.getReceiptExtractionMessage(result, appliedFields);
            this.changeDetectorRef.detectChanges();
          },
          error: (err: Error) => {
            if (requestId !== this.receiptExtractionRequestId) {
              return;
            }

            this.isExtractingReceiptData = false;
            this.receiptExtractionStatus = 'error';
            this.receiptExtractionMessage =
              err.message ||
              'Automatic field detection failed. Please fill in the fields manually.';
            this.changeDetectorRef.detectChanges();
          },
        });
    } catch (error) {
      this.isExtractingReceiptData = false;
      this.receiptExtractionStatus = 'error';
      this.receiptExtractionMessage =
        error instanceof Error
          ? error.message
          : 'Automatic field detection failed. Please fill in the fields manually.';
    }
  }

  private applyReceiptExtractionSuggestions(result: ReceiptExtractionDto): number {
    let appliedFields = 0;

    appliedFields += this.patchIfEmpty('amount', result.amount?.value);
    appliedFields += this.patchIfEmpty('paidAt', this.toDateInputValue(result.invoiceDate?.value));
    appliedFields += this.patchIfEmpty('invoiceNumber', result.invoiceNumber?.value);

    return appliedFields;
  }

  private getReceiptExtractionMessage(result: ReceiptExtractionDto, appliedFields: number): string {
    if (appliedFields > 0) {
      return `Pre-filled ${appliedFields} ${appliedFields === 1 ? 'field' : 'fields'} from the receipt. Please review before submitting.`;
    }

    if (this.hasExtractedReceiptValues(result)) {
      return 'Invoice details were detected, but your existing input was kept.';
    }

    return result.message ?? 'No reliable invoice details were detected.';
  }

  private hasExtractedReceiptValues(result: ReceiptExtractionDto): boolean {
    return [result.amount?.value, result.invoiceDate?.value, result.invoiceNumber?.value].some(
      (value) => value != null && value !== '',
    );
  }

  private patchIfEmpty(field: string, value: string | number | null | undefined): number {
    if (value == null || value === '') {
      return 0;
    }

    const control = this.form.get(field);
    if (!control || !this.isEmptyControlValue(control.value)) {
      return 0;
    }

    control.setValue(value);
    control.updateValueAndValidity();
    return 1;
  }

  private isEmptyControlValue(value: unknown): boolean {
    return value == null || (typeof value === 'string' && value.trim() === '');
  }

  private toDateInputValue(value: string | null | undefined): string | null {
    if (!value) {
      return null;
    }

    return value.slice(0, 10);
  }

  private clearReceiptExtractionState(): void {
    this.receiptExtractionRequestId++;
    this.isExtractingReceiptData = false;
    this.receiptExtractionStatus = 'idle';
    this.receiptExtractionMessage = '';
    this.receiptExtractionResult = null;
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
    if (this.form.invalid || (!this.isEditMode && !this.selectedFile)) return;

    this.isSubmitting = true;
    const v = this.form.value;
    const payoutType = this.toPayoutType(v.payoutType);

    if (payoutType === null) {
      this.notificationService.showError('Invalid payout type.');
      this.isSubmitting = false;
      return;
    }

    const payload = {
      invoiceNumber: v.invoiceNumber,
      comment: v.comment?.trim() || null,
      payoutType: payoutType,
      bankAccountId: payoutType === PayoutType.User ? Number(v.bankAccountId) : null,
      creditorName: payoutType === PayoutType.NotYetPaid ? v.creditorName : null,
      dueDate:
        payoutType === PayoutType.NotYetPaid && v.dueDate
          ? new Date(v.dueDate).toISOString()
          : null,
      receipt: '', // ignored — real file is passed separately below
      transaction: {
        teamId: Number(v.teamId),
        amount: Number(v.amount),
        purposeOfPayment: v.purposeOfPayment,
        paidAt: v.paidAt,
      },
    } as CreatePaymentRequestByUserDto;

    if (this.isEditMode) {
      this.resubmitPaymentRequest(payload);
      return;
    }

    if (this.offlineService.isOffline()) {
      void this.queueOfflineSubmission(payload, this.selectedFile!);
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
              this.duplicateSourceInvoice = this.buildDuplicateSourceInvoice(payload);
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

  private resubmitPaymentRequest(payload: CreatePaymentRequestByUserDto): void {
    this.paymentRequestByUserService
      .resubmitPaymentRequestByUser(this.editingInvoiceId!, payload, this.selectedFile)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Invoice updated and returned for review.');
          this.statusRefreshService.requestRefresh();
          this.isSubmitting = false;
          this.router.navigate(['/my-invoices', this.editingInvoiceId]);
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Invoice update failed.');
          this.isSubmitting = false;
          this.changeDetectorRef.detectChanges();
        },
      });
  }

  onCancelEdit(): void {
    this.router.navigate(['/my-invoices', this.editingInvoiceId]);
  }

  onDuplicateModalCancel(): void {
    this.isDuplicateModalOpen = false;
    this.duplicateCandidates = [];
    this.duplicateSourceInvoice = null;
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
    this.duplicateSourceInvoice = null;
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
    this.form.reset({
      invoiceNumber: '',
      comment: '',
      payoutType: this.getPayoutTypeControlValue(PayoutType.User),
      bankAccountId: null,
      creditorName: null,
      dueDate: null,
      teamId: null,
      amount: null,
      purposeOfPayment: '',
      paidAt: '',
      receipt: null,
    });
    this.authService.currentUser$.pipe(take(1)).subscribe((user) => {
      const userTeamId = user?.team?.id;
      if (userTeamId != null) {
        this.form.get('teamId')?.setValue(userTeamId);
      }
    });
    this.selectedFile = null;
    this.selectedFileName = '';
    this.duplicateCandidates = [];
    this.duplicateSourceInvoice = null;
    this.isDuplicateModalOpen = false;
    this.pendingSubmissionPayload = null;
    this.pendingSubmissionFile = null;
    this.isSubmitting = false;
    this.clearReceiptExtractionState();
    this.changeDetectorRef.detectChanges();
  }

  private applyOfflineDraftToForm(draft: OfflineInvoiceSubmissionDraft): void {
    const paidAt = draft.payload.transaction.paidAt ?? '';

    const dueDate = draft.payload.dueDate
      ? new Date(draft.payload.dueDate).toISOString().slice(0, 10)
      : '';

    this.form.patchValue({
      invoiceNumber: draft.payload.invoiceNumber,
      comment: draft.payload.comment ?? '',
      payoutType: this.getPayoutTypeControlValue(draft.payload.payoutType),
      bankAccountId: draft.payload.bankAccountId,
      creditorName: draft.payload.creditorName,
      dueDate,
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
    this.clearReceiptExtractionState();
    this.changeDetectorRef.detectChanges();
  }

  toPayoutType(value: unknown): PayoutType | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const num = Number(value);

    return Number.isNaN(num) || !Object.values(PayoutType).includes(num)
      ? null
      : (num as PayoutType);
  }

  getPayoutTypeControlValue(value: PayoutType | null | undefined): string | null {
    return value == null ? null : String(value);
  }

  isPayoutType(type: PayoutType): boolean {
    return this.toPayoutType(this.form.get('payoutType')?.value) === type;
  }

  private buildDuplicateSourceInvoice(
    payload: CreatePaymentRequestByUserDto,
  ): DuplicateInvoiceSummary {
    const team = this.teams.find((candidate) => candidate.id === payload.transaction.teamId);

    return {
      invoiceNumber: payload.invoiceNumber,
      amount: payload.transaction.amount,
      paidAt: payload.transaction.paidAt,
      purposeOfPayment: payload.transaction.purposeOfPayment,
      user: { name: this.currentUserName },
      team: { name: team?.name ?? 'Unknown team' },
    };
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
