import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, input } from '@angular/core';

import { AuthService } from '../../../../../services/auth/auth-service';
import {
  BankAccountDto,
  BankAccountService,
  CreateBankAccountRequestDto,
  UpdateBankAccountRequestDto,
} from '../../../../../services/bank-account/bank-account-service';
import { UserDto } from '../../../../../types/exporter';
import { BankAccountEditorComponent } from '../../../../bankaccount/bank-account-editor/bank-account-editor';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';
import { ModalComponent } from '../../../../general/modal-component/modal-component';

@Component({
  selector: 'app-bank-accounts-settings-page',
  imports: [BankAccountEditorComponent, BoxComponent, CommonModule, ModalComponent],
  templateUrl: './bank-accounts-settings-page.html',
  styleUrl: './bank-accounts-settings-page.scss',
})
export class BankAccountsSettingsPageComponent implements OnInit {
  user = input<UserDto | null>(null);

  public bankAccounts: BankAccountDto[] = [];
  public isLoading = false;
  public isCreateModalOpen = false;
  public editingBankAccount: BankAccountDto | null = null;
  public bankAccountPendingDelete: BankAccountDto | null = null;
  public errorMessage = '';
  public modalErrorMessage = '';

  constructor(
    private readonly bankAccountService: BankAccountService,
    private readonly authService: AuthService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadBankAccounts();
  }

  public loadBankAccounts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.bankAccountService.getBankAccounts().subscribe({
      next: (data: BankAccountDto[]) => {
        this.bankAccounts = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleError(error, 'Failed to load bank accounts');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  public openCreateModal(): void {
    this.editingBankAccount = null;
    this.modalErrorMessage = '';
    this.isCreateModalOpen = true;
    this.cdr.detectChanges();
  }

  public openEditModal(account: BankAccountDto): void {
    this.isCreateModalOpen = false;
    this.modalErrorMessage = '';
    this.editingBankAccount = account;
    this.cdr.detectChanges();
  }

  public closeModal(): void {
    this.isCreateModalOpen = false;
    this.editingBankAccount = null;
    this.modalErrorMessage = '';
    this.cdr.detectChanges();
  }

  public openDeleteModal(account: BankAccountDto): void {
    this.bankAccountPendingDelete = account;
    this.modalErrorMessage = '';
    this.cdr.detectChanges();
  }

  public closeDeleteModal(): void {
    this.bankAccountPendingDelete = null;
    this.modalErrorMessage = '';
    this.cdr.detectChanges();
  }

  public deleteBankAccount(): void {
    const id = this.bankAccountPendingDelete?.id;
    if (id == null) {
      this.handleModalError(new Error('Missing bank account id'), 'Failed to delete bank account');
      this.cdr.detectChanges();
      return;
    }

    this.bankAccountService.deleteBankAccount(id).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.loadBankAccounts();
        this.refreshCurrentUser();
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleModalError(error, 'Failed to delete bank account');
        this.cdr.detectChanges();
      },
    });
  }

  public createBankAccount(request: CreateBankAccountRequestDto): void {
    this.bankAccountService.createBankAccount(request).subscribe({
      next: () => {
        this.closeModal();
        this.loadBankAccounts();
        this.refreshCurrentUser();
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleModalError(error, 'Failed to create bank account');
        this.cdr.detectChanges();
      },
    });
  }

  public updateBankAccount(request: UpdateBankAccountRequestDto): void {
    if (!this.editingBankAccount) {
      return;
    }

    const id = this.editingBankAccount.id;
    if (id == null) {
      this.handleModalError(new Error('Missing bank account id'), 'Failed to update bank account');
      this.cdr.detectChanges();
      return;
    }

    this.bankAccountService.updateBankAccount(id, request).subscribe({
      next: () => {
        this.closeModal();
        this.loadBankAccounts();
        this.refreshCurrentUser();
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleModalError(error, 'Failed to update bank account');
        this.cdr.detectChanges();
      },
    });
  }

  public formatIbanForDisplay(iban: string | null | undefined): string {
    return (iban ?? '')
      .replaceAll(' ', '')
      .replace(/(.{4})/g, '$1 ')
      .trim();
  }

  private handleError(error: unknown, fallbackMessage: string): void {
    this.errorMessage = error instanceof Error ? error.message : fallbackMessage;
    console.error(fallbackMessage, error);
  }

  private handleModalError(error: unknown, fallbackMessage: string): void {
    this.modalErrorMessage = error instanceof Error ? error.message : fallbackMessage;
    console.error(fallbackMessage, error);
  }

  private refreshCurrentUser(): void {
    this.authService.refreshUser().catch((error: unknown) => {
      this.handleError(error, 'Failed to refresh current user');
      this.cdr.detectChanges();
    });
  }
}
