import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { Role, UserDto } from '../../../types/exporter';

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
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call auth service on logout', () => {
    component.openSignOutModal();
    expect((component as any).signOutModalOpen()).toBe(true);

    component.logout();

    expect(authServiceMock.logout).toHaveBeenCalled();
    expect((component as any).signOutModalOpen()).toBe(false);
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

  it('should load submitted invoice count for admins', () => {
    paymentServiceMock.getPaymentRequestsByUser.mockReturnValueOnce(of({ totalCount: 7 }));

    authServiceMock.currentUser$.next({
      id: 1,
      name: 'Admin User',
      email: 'admin@example.com',
      isActive: true,
      role: Role.ADMIN,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team' },
      bankInformationSkipped: false,
      hasBankInformation: true,
      bankAccounts: [],
    });

    fixture.detectChanges();

    expect(paymentServiceMock.getPaymentRequestsByUser).toHaveBeenCalledWith({
      Status: 0,
      Limit: 1,
    });
    expect((component as any).submittedCount()).toBe(7);
  });

  it('should load team request count for the current user', () => {
    teamRequestServiceMock.getPaymentRequestsByTeam.mockReturnValueOnce(of({ totalCount: 3 }));

    authServiceMock.currentUser$.next({
      id: 42,
      name: 'Regular User',
      email: 'user@example.com',
      isActive: true,
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      team: { id: 1, name: 'Team' },
      bankInformationSkipped: false,
      hasBankInformation: true,
      bankAccounts: [],
    });

    fixture.detectChanges();

    expect(teamRequestServiceMock.getPaymentRequestsByTeam).toHaveBeenCalledWith({
      Status: 0,
      UserId: 42,
      Limit: 1,
    });
    expect((component as any).teamRequestCount()).toBe(3);
  });

  it('should toggle and reset mobile and dropdown menu state', () => {
    component.toggleMobileMenu();
    component.toggleManagementMenu();
    component.toggleRequestsMenu();

    expect((component as any).mobileMenuOpen()).toBe(true);
    expect((component as any).managementMenuOpen()).toBe(true);
    expect((component as any).requestsMenuOpen()).toBe(true);

    component.closeMobileMenu();

    expect((component as any).mobileMenuOpen()).toBe(false);
    expect((component as any).managementMenuOpen()).toBe(false);
    expect((component as any).requestsMenuOpen()).toBe(false);
  });

  it('should toggle the sign out modal state', () => {
    component.openSignOutModal();
    expect((component as any).signOutModalOpen()).toBe(true);

    component.closeSignOutModal();
    expect((component as any).signOutModalOpen()).toBe(false);
  });

  it('should expand the management menu for management routes', () => {
    (component as any).currentUrl.set('/team');

    expect(component.isManagementMenuExpanded()).toBe(true);
    expect(component.isRequestsMenuExpanded()).toBe(false);
  });

  it('should expand the requests menu for request routes', () => {
    (component as any).currentUrl.set('/requests');

    expect(component.isRequestsMenuExpanded()).toBe(true);
    expect(component.isManagementMenuExpanded()).toBe(false);
  });
});
