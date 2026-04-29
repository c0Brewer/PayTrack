import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { BankAccountService } from '../../../services/bank-account/bank-account-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { TeamService } from '../../../services/team/team-service';
import {
  TeamDto,
  CreatePaymentRequestByUserDto,
  PayoutType,
  BankAccount,
} from '../../../types/exporter';
import {BoxComponent} from '../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-receipt-submit-component',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BoxComponent],
  templateUrl: './receipt-submit-component.html',
  styleUrl: './receipt-submit-component.scss',
})
export class ReceiptSubmitComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  teams: TeamDto[] = [];
  bankAccounts: BankAccount[] = [];
  isSubmitting = false;
  selectedFile: File | null = null;
  selectedFileName = '';

  readonly PayoutType = PayoutType;

  private readonly destroy$ = new Subject<void>();

  payoutTypeOptions = Object.values(PayoutType).filter(
    (v) => typeof v === 'number',
  ) as PayoutType[];

  payoutTypeLabels: Record<PayoutType, string> = {
    [PayoutType.User]: 'Already paid by user (you)',
    [PayoutType.External]: 'Not yet paid by user. Should be paid to invoice-issuer.',
  };

  constructor(
    private readonly fb: FormBuilder,
    private readonly paymentRequestByUserService: PaymentRequestByUserService,
    private readonly teamService: TeamService,
    private readonly bankAccountService: BankAccountService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
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
      teamId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      purposeOfPayment: ['', [Validators.required, Validators.maxLength(255)]],
      paidAt: ['', Validators.required],
      receipt: [null, Validators.required],
    });

    // add custom check for bank account (only trigger when payouttype is user)
    this.form
      .get('payoutType')!
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe((value) => {
        const bankCtrl = this.form.get('bankAccountId');

        if (value === PayoutType.User) {
          bankCtrl?.setValidators([Validators.required, Validators.min(1)]);
        } else {
          bankCtrl?.clearValidators();
          bankCtrl?.setValue(null); // reset value when hidden
        }

        bankCtrl?.updateValueAndValidity();
      });
  }

  private loadTeams(): void {
    this.teamService
      .getTeams({})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (teams) => {
          if (teams.items != null) {
            this.teams = teams.items;
          }
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
    if (errors['maxLength'])
      return `Maximum length is ${errors['maxLength'].requiredLength} characters.`;
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
      bankAccountId: Number(v.bankAccountId),
      receipt: '', // ignored — real file is passed separately below
      transaction: {
        teamId: Number(v.teamId),
        amount: Number(v.amount),
        purposeOfPayment: v.purposeOfPayment,
        paidAt: new Date(v.paidAt).toISOString(),
      },
    };

    this.paymentRequestByUserService
      .createPaymentRequestByUser(payload, this.selectedFile) // <- we pass the actual File here
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Invoice submitted successfully.');
          this.form.reset();
          this.selectedFile = null;
          this.selectedFileName = '';
          this.isSubmitting = false;
          this.router.navigate(['/']);
        },
        error: (err: Error) => {
          this.notificationService.showError(err.message ?? 'Submission failed.');
          this.isSubmitting = false;
        },
      });
  }

  toPayoutType(value: unknown): PayoutType | null {
    const num = Number(value);

    return Object.values(PayoutType).includes(num) ? (num as PayoutType) : null;
  }
}
