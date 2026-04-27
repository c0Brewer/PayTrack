import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';

import {
  BankAccountDto,
  BankAccountService,
  CreateBankAccountRequestDto,
  UpdateBankAccountRequestDto,
} from '../../../services/bank-account/bank-account-service';
import { BankAccountEditorComponent } from '../bank-account-editor/bank-account-editor';

@Component({
  selector: 'app-bank-account-component',
  standalone: true,
  imports: [CommonModule, BankAccountEditorComponent],
  templateUrl: './bank-account-component.html',
  styleUrl: './bank-account-component.scss',
})
export class BankAccountComponent implements OnInit {
  public bankAccounts: BankAccountDto[] = [];

  public isLoading = false;
  public isCreateModalOpen = false;
  public editingBankAccount: BankAccountDto | null = null;

  public errorMessage = '';
  public modalErrorMessage = '';

  constructor(
    private readonly bankAccountService: BankAccountService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  public ngOnInit(): void {
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

  public deleteBankAccount(id: number | undefined): void {
    if (id == null) {
      this.handleError(new Error('Missing bank account id'), 'Failed to delete bank account');
      this.cdr.detectChanges();
      return;
    }

    if (!confirm('Delete this bank account?')) {
      return;
    }

    this.bankAccountService.deleteBankAccount(id).subscribe({
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

  public createBankAccount(request: CreateBankAccountRequestDto): void {
    this.bankAccountService.createBankAccount(request).subscribe({
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
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.handleModalError(error, 'Failed to update bank account');
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
