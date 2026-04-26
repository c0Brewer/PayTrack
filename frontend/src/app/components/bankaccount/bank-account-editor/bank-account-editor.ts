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
  @Input() public title = 'Edit Bank Account';
  @Input() public submitLabel = 'Save';
  @Input() public initialValue: BankAccountDto | null = null;
  @Input() public errorMessage = '';

  @Output() public readonly cancel = new EventEmitter<void>();
  @Output() public readonly submitForm = new EventEmitter<CreateBankAccountRequestDto>();

  public readonly form: BankAccountForm;
  public validationMessage = '';

  constructor(private readonly fb: FormBuilder) {
    this.form = this.fb.nonNullable.group({
      accountHolder: [
        '',
        [Validators.required, Validators.minLength(3), Validators.maxLength(255)],
      ],
      iban: ['', [Validators.required, Validators.minLength(15), Validators.maxLength(31)]],
      bic: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(11)]],
    });

    this.form.valueChanges.subscribe(() => {
      this.validationMessage = '';
    });
  }

  public ngOnChanges(changes: SimpleChanges): void {
    if (!changes['initialValue']) {
      return;
    }

    if (this.initialValue) {
      this.form.setValue({
        accountHolder: this.initialValue.accountHolder ?? '',
        iban: this.initialValue.iban ?? '',
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
      this.validationMessage =
        'Please enter valid values (Account Holder 3-255, IBAN 15-31, BIC 8-11).';
      return;
    }

    this.submitForm.emit({
      accountHolder: this.form.controls.accountHolder.value.trim(),
      iban: this.form.controls.iban.value.replace(/\s+/g, '').toUpperCase(),
      bic: this.form.controls.bic.value.replace(/\s+/g, '').toUpperCase(),
    });
  }
}
