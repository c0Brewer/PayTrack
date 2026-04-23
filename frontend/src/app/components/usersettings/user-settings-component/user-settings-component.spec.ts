import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { UserSettingsComponent } from './user-settings-component';
import { UserService } from '../../../services/user/user-service';

describe('UserSettingsComponent', () => {
  let component: UserSettingsComponent;
  let fixture: ComponentFixture<UserSettingsComponent>;

  const userServiceMock = {
    getBankAccounts: () => of({ bankAccounts: [] }),
    createBankAccount: () => of({ id: 1, accountHolder: 'Test', iban: 'AT611904300234573201', bic: 'BKAUATWW' }),
    updateBankAccount: () => of({ id: 1, accountHolder: 'Test', iban: 'AT611904300234573201', bic: 'BKAUATWW' }),
    deleteBankAccount: () => of(void 0),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserSettingsComponent],
      providers: [{ provide: UserService, useValue: userServiceMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(UserSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
