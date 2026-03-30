import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { UserService } from '../../../services/user/user-service';
import { UserSettingsDto, BankAccountDto } from '../../../types/exporter';

import { AuthService } from '../../../services/auth/auth-service';

@Component({
  selector: 'app-user-settings-component',
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './user-settings-component.html',
  styleUrl: './user-settings-component.scss',
})
export class UserSettingsComponent implements OnInit {
  public settingsForm!: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly userSettingsService: UserService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  private initForm(): void {
    this.settingsForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      preferredBankAccountId: [null],
      bankAccounts: this.fb.array([])
    });
  }

  get bankAccounts(): FormArray {
    return this.settingsForm.get('bankAccounts') as FormArray;
  }

  private loadSettings(): void {
    this.userSettingsService.getUserSettings().subscribe({
      next: (data: UserSettingsDto) => {
        this.bankAccounts.clear();
        if (data.bankAccounts) {
          data.bankAccounts.forEach(account => this.addBankAccount(account));
        }

        this.settingsForm.patchValue({
          name: data.name,
          email: data.email,
          preferredBankAccountId: data.preferredBankAccountId
        });
      },
      error: (err) => console.error('Failed to load settings', err),
    });
  }

  public addBankAccount(account?: BankAccountDto): void {
    const accountForm = this.fb.group({
      id: [account ? account.id : 0], // 0 for new accounts
      accountHolder: [account ? account.accountHolder : '', Validators.required],
      iban: [account ? account.iban : '', Validators.required],
      bic: [account ? account.bic : '', Validators.required]
    });

    this.bankAccounts.push(accountForm);
  }

  public removeBankAccount(index: number): void {
    const accountId = this.bankAccounts.at(index).get('id')?.value;

    // Clear preferred account if it's the one being deleted
    if (this.settingsForm.get('preferredBankAccountId')?.value === accountId) {
      this.settingsForm.patchValue({ preferredBankAccountId: null });
    }

    this.bankAccounts.removeAt(index);
  }

  public trackByFn(index: number, item: any): any {
    return item.get('id')?.value || index;
  }

  public onSubmit(): void {
    if (this.settingsForm.invalid) return;

    this.userSettingsService.updateUserSettings(this.settingsForm.value).subscribe({
      next: () => {
        alert('Settings saved!');
        this.loadSettings();
      },
      error: (err) => console.error('Failed to save settings', err),
    });
  }
}
