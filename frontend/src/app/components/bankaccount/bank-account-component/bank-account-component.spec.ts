//AI helped with the test cases

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChangeDetectorRef } from '@angular/core';
import { of, throwError } from 'rxjs';

import { BankAccountComponent } from './bank-account-component';
import { BankAccountService } from '../../../services/bank-account/bank-account-service';

describe('BankAccountComponent', () => {
  let component: BankAccountComponent;
  let fixture: ComponentFixture<BankAccountComponent>;
  let bankAccountServiceMock: {
    getBankAccounts: ReturnType<typeof vi.fn>;
    createBankAccount: ReturnType<typeof vi.fn>;
    updateBankAccount: ReturnType<typeof vi.fn>;
    deleteBankAccount: ReturnType<typeof vi.fn>;
  };
  let cdrMock: {
    detectChanges: ReturnType<typeof vi.fn>;
  };

  const mockAccount = {
    id: 1,
    accountHolder: 'Test',
    iban: 'AT611904300234573201',
    bic: 'BKAUATWW',
  };

  beforeEach(async () => {
    bankAccountServiceMock = {
      getBankAccounts: vi.fn().mockReturnValue(of([])),
      createBankAccount: vi.fn().mockReturnValue(of(mockAccount)),
      updateBankAccount: vi.fn().mockReturnValue(of(mockAccount)),
      deleteBankAccount: vi.fn().mockReturnValue(of(void 0)),
    };

    cdrMock = {
      detectChanges: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [BankAccountComponent],
      providers: [
        { provide: BankAccountService, useValue: bankAccountServiceMock },
        { provide: ChangeDetectorRef, useValue: cdrMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BankAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should call loadBankAccounts', () => {
    component.ngOnInit();
    expect(bankAccountServiceMock.getBankAccounts).toHaveBeenCalled();
  });

  it('loadBankAccounts should update state on success', () => {
    bankAccountServiceMock.getBankAccounts.mockReturnValueOnce(of([mockAccount]));
    component.loadBankAccounts();

    expect(component.bankAccounts).toEqual([mockAccount]);
    expect(component.isLoading).toBe(false);
    expect(component.errorMessage).toBe('');
  });

  it('loadBankAccounts should set error message on failure', () => {
    bankAccountServiceMock.getBankAccounts.mockReturnValueOnce(
      throwError(() => new Error('load failed')),
    );
    component.loadBankAccounts();

    expect(component.errorMessage).toBe('load failed');
    expect(component.isLoading).toBe(false);
  });

  it('openCreateModal should open create modal and reset edit state', () => {
    component.editingBankAccount = mockAccount;
    component.modalErrorMessage = 'old';

    component.openCreateModal();

    expect(component.isCreateModalOpen).toBe(true);
    expect(component.editingBankAccount).toBeNull();
    expect(component.modalErrorMessage).toBe('');
  });

  it('openEditModal should set editing bank account', () => {
    component.openEditModal(mockAccount);

    expect(component.isCreateModalOpen).toBe(false);
    expect(component.editingBankAccount).toEqual(mockAccount);
  });

  it('closeModal should reset modal state', () => {
    component.isCreateModalOpen = true;
    component.editingBankAccount = mockAccount;
    component.modalErrorMessage = 'some error';

    component.closeModal();

    expect(component.isCreateModalOpen).toBe(false);
    expect(component.editingBankAccount).toBeNull();
    expect(component.modalErrorMessage).toBe('');
  });

  it('deleteBankAccount should set error when id is missing', () => {
    component.deleteBankAccount(undefined);

    expect(component.errorMessage).toBe('Missing bank account id');
    expect(bankAccountServiceMock.deleteBankAccount).not.toHaveBeenCalled();
  });

  it('deleteBankAccount should not call service when confirm is false', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.deleteBankAccount(1);

    expect(bankAccountServiceMock.deleteBankAccount).not.toHaveBeenCalled();
  });

  it('deleteBankAccount should call service when confirm is true', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => {});

    component.deleteBankAccount(1);

    expect(bankAccountServiceMock.deleteBankAccount).toHaveBeenCalledWith(1);
    expect(loadSpy).toHaveBeenCalled();
  });

  it('createBankAccount should call service and close modal on success', () => {
    const closeSpy = vi.spyOn(component, 'closeModal').mockImplementation(() => {});
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => {});

    component.createBankAccount({
      accountHolder: 'Max',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    });

    expect(bankAccountServiceMock.createBankAccount).toHaveBeenCalled();
    expect(closeSpy).toHaveBeenCalled();
    expect(loadSpy).toHaveBeenCalled();
  });

  it('createBankAccount should set modal error message on failure', () => {
    bankAccountServiceMock.createBankAccount.mockReturnValueOnce(
      throwError(() => new Error('create failed')),
    );

    component.createBankAccount({
      accountHolder: 'Max',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    });

    expect(component.modalErrorMessage).toBe('create failed');
  });

  it('updateBankAccount should return when no account is being edited', () => {
    component.editingBankAccount = null;

    component.updateBankAccount({ bic: 'NEWBIC12' });

    expect(bankAccountServiceMock.updateBankAccount).not.toHaveBeenCalled();
  });

  it('updateBankAccount should set modal error when id is missing', () => {
    component.editingBankAccount = {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      id: undefined as any,
      accountHolder: 'A',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    };

    component.updateBankAccount({ bic: 'NEWBIC12' });

    expect(component.modalErrorMessage).toBe('Missing bank account id');
  });

  it('updateBankAccount should call service and reload on success', () => {
    component.editingBankAccount = { ...mockAccount };
    const closeSpy = vi.spyOn(component, 'closeModal').mockImplementation(() => {});
    const loadSpy = vi.spyOn(component, 'loadBankAccounts').mockImplementation(() => {});

    component.updateBankAccount({ bic: 'NEWBIC12' });

    expect(bankAccountServiceMock.updateBankAccount).toHaveBeenCalledWith(1, { bic: 'NEWBIC12' });
    expect(closeSpy).toHaveBeenCalled();
    expect(loadSpy).toHaveBeenCalled();
  });

  it('updateBankAccount should set modal error on failure', () => {
    component.editingBankAccount = { ...mockAccount };
    bankAccountServiceMock.updateBankAccount.mockReturnValueOnce(
      throwError(() => new Error('update failed')),
    );

    component.updateBankAccount({ bic: 'NEWBIC12' });

    expect(component.modalErrorMessage).toBe('update failed');
  });
});
