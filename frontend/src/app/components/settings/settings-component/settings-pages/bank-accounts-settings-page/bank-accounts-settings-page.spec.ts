import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../../../services/auth/auth-service';
import {
  BankAccountDto,
  BankAccountService,
  CreateBankAccountRequestDto,
  UpdateBankAccountRequestDto,
} from '../../../../../services/bank-account/bank-account-service';

import { BankAccountsSettingsPageComponent } from './bank-accounts-settings-page';

describe('BankAccountsSettingsPageComponent', () => {
  let fixture: ComponentFixture<BankAccountsSettingsPageComponent>;
  let component: BankAccountsSettingsPageComponent;
  let bankAccountServiceMock: {
    getBankAccounts: ReturnType<typeof vi.fn>;
    deleteBankAccount: ReturnType<typeof vi.fn>;
    createBankAccount: ReturnType<typeof vi.fn>;
    updateBankAccount: ReturnType<typeof vi.fn>;
  };
  let authServiceMock: {
    refreshUser: ReturnType<typeof vi.fn>;
  };
  let detectChangesSpy: ReturnType<typeof vi.spyOn>;

  const mockAccount: BankAccountDto = {
    id: 7,
    accountHolder: 'Alex Example',
    iban: 'AT611904300234573201',
    bic: 'BKAUATWW',
  };

  beforeEach(async () => {
    bankAccountServiceMock = {
      getBankAccounts: vi.fn(),
      deleteBankAccount: vi.fn(),
      createBankAccount: vi.fn(),
      updateBankAccount: vi.fn(),
    };
    authServiceMock = {
      refreshUser: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [BankAccountsSettingsPageComponent],
      providers: [
        { provide: BankAccountService, useValue: bankAccountServiceMock },
        { provide: AuthService, useValue: authServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BankAccountsSettingsPageComponent);
    component = fixture.componentInstance;
    detectChangesSpy = vi.spyOn((component as never as { cdr: { detectChanges: () => void } }).cdr, 'detectChanges');
  });

  it('should call loadBankAccounts on init', () => {
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => undefined);

    component.ngOnInit();

    expect(loadSpy).toHaveBeenCalled();
  });

  it('should load bank accounts successfully', () => {
    bankAccountServiceMock.getBankAccounts.mockReturnValue(of([mockAccount]));

    component.loadBankAccounts();

    expect(component.isLoading).toBe(false);
    expect(component.errorMessage).toBe('');
    expect(component.bankAccounts).toEqual([mockAccount]);
    expect(detectChangesSpy).toHaveBeenCalled();
  });

  it('should set error message when loading bank accounts fails with Error', () => {
    const error = new Error('load failed');
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    bankAccountServiceMock.getBankAccounts.mockReturnValue(throwError(() => error));

    component.loadBankAccounts();

    expect(component.isLoading).toBe(false);
    expect(component.errorMessage).toBe('load failed');
    expect(detectChangesSpy).toHaveBeenCalled();
    expect(consoleSpy).toHaveBeenCalledWith('Failed to load bank accounts', error);
  });

  it('should use fallback message when loading bank accounts fails with non-Error', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    bankAccountServiceMock.getBankAccounts.mockReturnValue(throwError(() => 'boom'));

    component.loadBankAccounts();

    expect(component.errorMessage).toBe('Failed to load bank accounts');
    expect(consoleSpy).toHaveBeenCalledWith('Failed to load bank accounts', 'boom');
  });

  it('should open and close the create modal', () => {
    component.editingBankAccount = mockAccount;
    component.modalErrorMessage = 'old error';

    component.openCreateModal();

    expect(component.isCreateModalOpen).toBe(true);
    expect(component.editingBankAccount).toBeNull();
    expect(component.modalErrorMessage).toBe('');

    component.closeModal();

    expect(component.isCreateModalOpen).toBe(false);
    expect(component.editingBankAccount).toBeNull();
    expect(component.modalErrorMessage).toBe('');
    expect(detectChangesSpy).toHaveBeenCalledTimes(2);
  });

  it('should open edit and delete modals and close delete modal', () => {
    component.modalErrorMessage = 'old error';

    component.openEditModal(mockAccount);

    expect(component.isCreateModalOpen).toBe(false);
    expect(component.editingBankAccount).toEqual(mockAccount);
    expect(component.modalErrorMessage).toBe('');

    component.openDeleteModal(mockAccount);

    expect(component.bankAccountPendingDelete).toEqual(mockAccount);
    expect(component.modalErrorMessage).toBe('');

    component.closeDeleteModal();

    expect(component.bankAccountPendingDelete).toBeNull();
    expect(component.modalErrorMessage).toBe('');
  });

  it('should set modal error when deleting without an id', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    component.bankAccountPendingDelete = { ...mockAccount, id: undefined as unknown as number };

    component.deleteBankAccount();

    expect(component.modalErrorMessage).toBe('Missing bank account id');
    expect(consoleSpy).toHaveBeenCalled();
    expect(detectChangesSpy).toHaveBeenCalled();
  });

  it('should delete bank account successfully and refresh state', () => {
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => undefined);
    component.bankAccountPendingDelete = mockAccount;
    bankAccountServiceMock.deleteBankAccount.mockReturnValue(of(undefined));

    component.deleteBankAccount();

    expect(bankAccountServiceMock.deleteBankAccount).toHaveBeenCalledWith(7);
    expect(component.bankAccountPendingDelete).toBeNull();
    expect(loadSpy).toHaveBeenCalled();
    expect(authServiceMock.refreshUser).toHaveBeenCalled();
  });

  it('should set modal error when deleting fails', () => {
    const error = new Error('delete failed');
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    component.bankAccountPendingDelete = mockAccount;
    bankAccountServiceMock.deleteBankAccount.mockReturnValue(throwError(() => error));

    component.deleteBankAccount();

    expect(component.modalErrorMessage).toBe('delete failed');
    expect(consoleSpy).toHaveBeenCalledWith('Failed to delete bank account', error);
  });

  it('should create bank account successfully and refresh state', () => {
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => undefined);
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as CreateBankAccountRequestDto;
    bankAccountServiceMock.createBankAccount.mockReturnValue(of(undefined));

    component.isCreateModalOpen = true;
    component.createBankAccount(request);

    expect(bankAccountServiceMock.createBankAccount).toHaveBeenCalledWith(request);
    expect(component.isCreateModalOpen).toBe(false);
    expect(loadSpy).toHaveBeenCalled();
    expect(authServiceMock.refreshUser).toHaveBeenCalled();
  });

  it('should set modal error when creating bank account fails', () => {
    const error = new Error('create failed');
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as CreateBankAccountRequestDto;
    bankAccountServiceMock.createBankAccount.mockReturnValue(throwError(() => error));

    component.createBankAccount(request);

    expect(component.modalErrorMessage).toBe('create failed');
    expect(consoleSpy).toHaveBeenCalledWith('Failed to create bank account', error);
  });

  it('should ignore update requests when no account is being edited', () => {
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as UpdateBankAccountRequestDto;

    component.updateBankAccount(request);

    expect(bankAccountServiceMock.updateBankAccount).not.toHaveBeenCalled();
  });

  it('should set modal error when updating without an id', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as UpdateBankAccountRequestDto;
    component.editingBankAccount = { ...mockAccount, id: undefined as unknown as number };

    component.updateBankAccount(request);

    expect(component.modalErrorMessage).toBe('Missing bank account id');
    expect(consoleSpy).toHaveBeenCalled();
  });

  it('should update bank account successfully and refresh state', () => {
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => undefined);
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as UpdateBankAccountRequestDto;
    component.editingBankAccount = mockAccount;
    bankAccountServiceMock.updateBankAccount.mockReturnValue(of(undefined));

    component.updateBankAccount(request);

    expect(bankAccountServiceMock.updateBankAccount).toHaveBeenCalledWith(7, request);
    expect(component.editingBankAccount).toBeNull();
    expect(loadSpy).toHaveBeenCalled();
    expect(authServiceMock.refreshUser).toHaveBeenCalled();
  });

  it('should set modal error when updating bank account fails', () => {
    const error = new Error('update failed');
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as UpdateBankAccountRequestDto;
    component.editingBankAccount = mockAccount;
    bankAccountServiceMock.updateBankAccount.mockReturnValue(throwError(() => error));

    component.updateBankAccount(request);

    expect(component.modalErrorMessage).toBe('update failed');
    expect(consoleSpy).toHaveBeenCalledWith('Failed to update bank account', error);
  });

  it('should format ibans in groups of four characters', () => {
    expect(component.formatIbanForDisplay('AT611904300234573201')).toBe('AT61 1904 3002 3457 3201');
    expect(component.formatIbanForDisplay('AT61 1904 3002 3457 3201')).toBe('AT61 1904 3002 3457 3201');
    expect(component.formatIbanForDisplay(null)).toBe('');
  });

  it('should handle refresh user failures after successful create', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    const request = {
      accountHolder: 'Alex Example',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    } as CreateBankAccountRequestDto;
    bankAccountServiceMock.createBankAccount.mockReturnValue(of(undefined));
    bankAccountServiceMock.getBankAccounts.mockReturnValue(of([mockAccount]));
    authServiceMock.refreshUser.mockRejectedValue(new Error('refresh failed'));

    component.createBankAccount(request);
    await Promise.resolve();

    expect(component.errorMessage).toBe('refresh failed');
    expect(consoleSpy).toHaveBeenCalledWith('Failed to refresh current user', expect.any(Error));
  });
});
