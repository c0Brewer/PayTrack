import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import {
  BankAccountDto,
  CreateBankAccountRequestDto,
} from '../../../services/bank-account/bank-account-service';
import { ModalComponent } from '../../general/modal-component/modal-component';

type BankAccountForm = FormGroup<{
  accountHolder: FormControl<string>;
  iban: FormControl<string>;
  bic: FormControl<string>;
}>;

@Component({
  selector: 'app-bank-account-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ModalComponent],
  templateUrl: './bank-account-editor.html',
  styleUrl: './bank-account-editor.scss',
})
export class BankAccountEditorComponent implements OnChanges {
  private static readonly invalidIbanMessage = 'IBAN is invalid.';

  @Input() public title = 'Edit Bank Account';
  @Input() public submitLabel = 'Save';
  @Input() public initialValue: BankAccountDto | null = null;
  @Input() public errorMessage = '';

  @Output() public readonly cancel = new EventEmitter<void>();
  @Output() public readonly submitForm = new EventEmitter<CreateBankAccountRequestDto>();

  public readonly form: BankAccountForm;

  constructor(private readonly fb: FormBuilder) {
    this.form = this.fb.nonNullable.group({
      accountHolder: ['', [Validators.required, Validators.maxLength(255)]],
      iban: [
        '',
        [
          Validators.required,
          Validators.maxLength(42),
          Validators.pattern(/^[A-Za-z]{2}[A-Za-z0-9 ]{13,32}$/),
        ],
      ],
      bic: [
        '',
        [
          Validators.required,
          Validators.maxLength(11),
          Validators.pattern(/^[A-Za-z]{4}[A-Za-z]{2}[A-Za-z0-9]{2}([A-Za-z0-9]{3})?$/),
        ],
      ],
    });
  }

  public ngOnChanges(changes: SimpleChanges): void {
    if (!changes['initialValue']) {
      return;
    }

    if (this.initialValue) {
      this.form.setValue({
        accountHolder: this.initialValue.accountHolder ?? '',
        iban: this.formatIban(this.initialValue.iban ?? ''),
        bic: this.initialValue.bic ?? '',
      });
      return;
    }

    this.form.reset({
      accountHolder: '',
      iban: '',
      bic: '',
    });
  }

  public onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitForm.emit({
      accountHolder: this.form.controls.accountHolder.value.trim(),
      iban: this.form.controls.iban.value.replace(/\s+/g, '').toUpperCase(),
      bic: this.form.controls.bic.value.replace(/\s+/g, '').toUpperCase(),
    });
  }

  public onIbanInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const formattedIban = this.formatIban(input.value);

    input.value = formattedIban;
    this.form.controls.iban.setValue(formattedIban, { emitEvent: false });
  }

  public get topLevelErrorMessage(): string {
    return this.errorMessage === BankAccountEditorComponent.invalidIbanMessage
      ? ''
      : this.errorMessage;
  }

  public get ibanErrorMessage(): string {
    if (this.errorMessage === BankAccountEditorComponent.invalidIbanMessage) {
      return this.errorMessage;
    }

    if (this.form.controls.iban.touched && this.form.controls.iban.invalid) {
      return 'Use 15-34 characters.';
    }

    return '';
  }

  private formatIban(value: string): string {
    return value
      .replaceAll(' ', '')
      .replace(/[^A-Za-z0-9]/g, '')
      .toUpperCase()
      .replace(/(.{4})/g, '$1 ')
      .trim();
  }
}
