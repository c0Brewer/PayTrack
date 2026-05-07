import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { BankAccountService } from '../../../services/bank-account/bank-account-service';
import { NotificationService } from '../../../services/notification/notification-service';

@Component({
  selector: 'app-initial-login-bank-account',
  imports: [ReactiveFormsModule],
  templateUrl: './initial-login-bank-account-component.html',
  styleUrl: './initial-login-bank-account-component.scss',
})
export class InitialLoginBankAccountComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);

  protected readonly form = this.formBuilder.nonNullable.group({
    accountHolder: ['', [Validators.required, Validators.maxLength(255)]],
    iban: [
      '',
      [
        Validators.required,
        Validators.maxLength(34),
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

  protected isSaving = false;

  constructor(
    private readonly authService: AuthService,
    private readonly bankAccountService: BankAccountService,
    private readonly router: Router,
    private readonly notificationService: NotificationService,
  ) {}

  async ngOnInit(): Promise<void> {
    const user = await this.authService.refreshUser();

    if (!this.authService.needsBankInformation(user)) {
      this.router.navigate(['']);
    }
  }

  protected async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    try {
      const value = this.form.getRawValue();
      await firstValueFrom(
        this.bankAccountService.createBankAccount({
          accountHolder: value.accountHolder.trim(),
          iban: value.iban.replaceAll(' ', '').toUpperCase(),
          bic: value.bic.replaceAll(' ', '').toUpperCase(),
        }),
      );
      await this.authService.refreshUser();
      this.router.navigate(['']);
    } catch (error) {
      this.notificationService.showError(this.getErrorMessage(error));
    } finally {
      this.isSaving = false;
    }
  }

  protected async skip(): Promise<void> {
    this.isSaving = true;

    try {
      await this.authService.skipBankInformation();
      this.router.navigate(['']);
    } catch (error) {
      this.notificationService.showError(this.getErrorMessage(error));
    } finally {
      this.isSaving = false;
    }
  }

  private getErrorMessage(error: unknown): string {
    return error instanceof Error ? error.message : String(error);
  }
}
