import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import { BankAccountEditorModalComponent } from '../bank-account-editor-modal/bank-account-editor-modal';
import {
  BankAccountDto,
  BankAccountRequestDto,
  BankAccountsResponseDto,
  UserService,
} from '../../../services/user/user-service';

@Component({
  selector: 'app-user-settings-component',
  standalone: true,
  imports: [CommonModule, BankAccountEditorModalComponent],
  templateUrl: './user-settings-component.html',
  styleUrl: './user-settings-component.scss',
})
export class UserSettingsComponent implements OnInit {
  public bankAccounts: BankAccountDto[] = [];

  public isLoading = false;
  public isCreateModalOpen = false;
  public editingBankAccount: BankAccountDto | null = null;

  public errorMessage = '';
  public modalErrorMessage = '';

  constructor(
    private readonly userService: UserService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  public ngOnInit(): void {
    this.loadBankAccounts();
  }

  public loadBankAccounts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getBankAccounts().subscribe({
      next: (data: BankAccountsResponseDto) => {
        this.bankAccounts = data.bankAccounts;
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

  public createBankAccount(request: BankAccountRequestDto): void {
    this.userService.createBankAccount(request).subscribe({
      next: () => {
        this.closeModal();
        this.loadBankAccounts();
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleModalError(error, 'Failed to create bank account');
        this.cdr.detectChanges();
      },
    });
  }

  public updateBankAccount(request: BankAccountRequestDto): void {
    if (!this.editingBankAccount) {
      return;
    }

    this.userService.updateBankAccount(this.editingBankAccount.id, request).subscribe({
      next: () => {
        this.closeModal();
        this.loadBankAccounts();
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleModalError(error, 'Failed to update bank account');
        this.cdr.detectChanges();
      },
    });
  }

  public deleteBankAccount(id: number): void {
    if (!confirm('Delete this bank account?')) {
      return;
    }

    this.userService.deleteBankAccount(id).subscribe({
      next: () => {
        this.loadBankAccounts();
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleError(error, 'Failed to delete bank account');
        this.cdr.detectChanges();
      },
    });
  }

  private handleError(error: unknown, fallbackMessage: string): void {
    this.errorMessage = error instanceof Error ? error.message : fallbackMessage;
    console.error(fallbackMessage, error);
  }

  private handleModalError(error: unknown, fallbackMessage: string): void {
    this.modalErrorMessage = error instanceof Error ? error.message : fallbackMessage;
    console.error(fallbackMessage, error);
  }
}
