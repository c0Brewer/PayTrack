import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { BankAccountComponent } from './bank-account-component';
import { BankAccountService } from '../../../services/bank-account/bank-account-service';

describe('BankAccountComponent', () => {
  let component: BankAccountComponent;
  let fixture: ComponentFixture<BankAccountComponent>;

  const bankAccountServiceMock = {
    getBankAccounts: () => of([]),
    createBankAccount: () => of({ id: 1, accountHolder: 'Test', iban: 'AT611904300234573201', bic: 'BKAUATWW' }),
    updateBankAccount: () => of({ id: 1, accountHolder: 'Test', iban: 'AT611904300234573201', bic: 'BKAUATWW' }),
    deleteBankAccount: () => of(void 0),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankAccountComponent],
      providers: [{ provide: BankAccountService, useValue: bankAccountServiceMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(BankAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
