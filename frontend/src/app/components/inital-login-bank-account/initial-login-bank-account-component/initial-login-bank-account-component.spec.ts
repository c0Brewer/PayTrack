import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { AuthService } from '../../../services/auth/auth-service';
import { BankAccountService } from '../../../services/bank-account/bank-account-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { Role, UserDto } from '../../../types/exporter';

import { InitialLoginBankAccountComponent } from './initial-login-bank-account-component';

describe('InitialLoginBankAccountComponent', () => {
  let component: InitialLoginBankAccountComponent;
  let fixture: ComponentFixture<InitialLoginBankAccountComponent>;

  const userNeedingBankInfo: UserDto = {
    id: 1,
    name: 'name',
    email: 'email',
    isActive: true,
    team: { id: -1, name: 'team' },
    role: Role.REGULAR_USER,
    profilePictureUrl: '',
    bankInformationSkipped: false,
    hasBankInformation: false,
    bankAccounts: [],
  };

  let authServiceMock: {
    refreshUser: ReturnType<typeof vi.fn>;
    needsBankInformation: ReturnType<typeof vi.fn>;
    skipBankInformation: ReturnType<typeof vi.fn>;
  };

  let bankAccountServiceMock: {
    createBankAccount: ReturnType<typeof vi.fn>;
  };

  let routerMock: {
    navigate: ReturnType<typeof vi.fn>;
  };

  let notificationServiceMock: {
    showError: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    authServiceMock = {
      refreshUser: vi.fn().mockResolvedValue(userNeedingBankInfo),
      needsBankInformation: vi.fn().mockReturnValue(true),
      skipBankInformation: vi.fn().mockResolvedValue(userNeedingBankInfo),
    };

    bankAccountServiceMock = {
      createBankAccount: vi.fn().mockReturnValue(
        of({
          id: 9,
          accountHolder: 'Max Mustermann',
          iban: 'AT611904300234573201',
          bic: 'BKAUATWW',
        }),
      ),
    };

    routerMock = {
      navigate: vi.fn(),
    };

    notificationServiceMock = {
      showError: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [InitialLoginBankAccountComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: BankAccountService, useValue: bankAccountServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(InitialLoginBankAccountComponent);
    component = fixture.componentInstance;
  });

  it('should redirect away on init if bank information is not needed', async () => {
    authServiceMock.needsBankInformation.mockReturnValue(false);

    await component.ngOnInit();

    expect(routerMock.navigate).toHaveBeenCalledWith(['']);
  });

  it('should normalize and save bank information', async () => {
    component['form'].setValue({
      accountHolder: '  Max Mustermann  ',
      iban: 'at61 1904 3002 3457 3201',
      bic: 'bkauatww',
    });

    await component['save']();

    expect(bankAccountServiceMock.createBankAccount).toHaveBeenCalledWith({
      accountHolder: 'Max Mustermann',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    });
    expect(routerMock.navigate).toHaveBeenCalledWith(['']);
  });

  it('should not save invalid form data', async () => {
    component['form'].setValue({
      accountHolder: '',
      iban: 'invalid',
      bic: '123',
    });

    await component['save']();

    expect(bankAccountServiceMock.createBankAccount).not.toHaveBeenCalled();
  });

  it('should skip bank information and navigate home', async () => {
    await component['skip']();

    expect(authServiceMock.skipBankInformation).toHaveBeenCalledOnce();
    expect(routerMock.navigate).toHaveBeenCalledWith(['']);
  });

  it('should show an error when save fails', async () => {
    bankAccountServiceMock.createBankAccount.mockReturnValueOnce(
      throwError(() => new Error('save failed')),
    );
    component['form'].setValue({
      accountHolder: 'Max Mustermann',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    });

    await component['save']();

    expect(notificationServiceMock.showError).toHaveBeenCalledWith('save failed');
  });
});
