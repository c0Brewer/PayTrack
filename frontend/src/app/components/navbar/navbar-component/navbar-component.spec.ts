import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestStatusRefreshService } from '../../../services/payment-request-by-user/payment-request-status-refresh-service';
import { Role, TransactionStatus, UserDto } from '../../../types/exporter';

import { NavbarComponent } from './navbar-component';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

  let paymentServiceMock: {
    getPaymentRequestsByUser: ReturnType<typeof vi.fn>;
  };
  let teamRequestServiceMock: {
    getPaymentRequestsByTeam: ReturnType<typeof vi.fn>;
  };
  let authServiceMock: {
    loggedIn$: BehaviorSubject<boolean>;
    currentUser$: BehaviorSubject<UserDto | null>;
    logout: ReturnType<typeof vi.fn>;
  };
  let statusRefreshService: PaymentRequestStatusRefreshService;

  beforeEach(async () => {
    authServiceMock = {
      loggedIn$: new BehaviorSubject<boolean>(true),
      currentUser$: new BehaviorSubject<UserDto | null>(null),
      logout: vi.fn(),
    };
    paymentServiceMock = {
      getPaymentRequestsByUser: vi.fn(() => of({ totalCount: 0 })),
    };
    teamRequestServiceMock = {
      getPaymentRequestsByTeam: vi.fn(() => of({ totalCount: 0 })),
    };
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        provideRouter([]),
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
        { provide: PaymentRequestByTeamService, useValue: teamRequestServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    statusRefreshService = TestBed.inject(PaymentRequestStatusRefreshService);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call auth service on logout', () => {
    component.logout();

    expect(authServiceMock.logout).toHaveBeenCalled();
  });

  it('should set hasNoBankAccounts when the current user has no bank accounts', () => {
    authServiceMock.currentUser$.next({
      id: 1,
      name: 'Test User',
      email: 'test@example.com',
      isActive: true,
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team' },
      bankInformationSkipped: false,
      hasBankInformation: false,
      bankAccounts: [],
    });

    expect(component.hasNoBankAccounts).toBe(true);
  });

  it('should unset hasNoBankAccounts when the current user has bank accounts', () => {
    authServiceMock.currentUser$.next({
      id: 1,
      name: 'Test User',
      email: 'test@example.com',
      isActive: true,
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team' },
      bankInformationSkipped: false,
      hasBankInformation: true,
      bankAccounts: [
        {
          id: 1,
          accountHolder: 'Test User',
          iban: 'AT611904300234573201',
          bic: 'BKAUATWW',
        },
      ],
    });

    expect(component.hasNoBankAccounts).toBe(false);
  });

  it('should show the number of invoices with requested changes in the navigation', async () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ totalCount: 3 }));
    fixture.detectChanges();

    authServiceMock.currentUser$.next({
      id: 7,
      name: 'Test User',
      email: 'test@example.com',
      isActive: true,
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team' },
      bankInformationSkipped: false,
      hasBankInformation: false,
      bankAccounts: [],
    });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalledWith({
      Status: TransactionStatus.ChangesRequested,
      UserId: 7,
      Limit: 1,
    });

    const myInvoicesLink = fixture.nativeElement.querySelector('a[href="/my-invoices"]');
    expect(myInvoicesLink.querySelector('.nav-badge').textContent.trim()).toBe('3');
  });

  it('should reload requested changes count when a status refresh is requested', async () => {
    authServiceMock.currentUser$.next({
      id: 7,
      name: 'Test User',
      email: 'test@example.com',
      isActive: true,
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team' },
      bankInformationSkipped: false,
      hasBankInformation: false,
      bankAccounts: [],
    });
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValue(of({ totalCount: 4 }));
    fixture.detectChanges();

    statusRefreshService.requestRefresh();
    await fixture.whenStable();
    fixture.detectChanges();

    const myInvoicesLink = fixture.nativeElement.querySelector('a[href="/my-invoices"]');
    expect(myInvoicesLink.querySelector('.nav-badge').textContent.trim()).toBe('4');
  });
});
