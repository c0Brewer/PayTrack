import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { Role, UserDto } from '../../../types/exporter';

import { NavbarComponent } from './navbar-component';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

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

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [{ provide: AuthService, useValue: authServiceMock }, provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
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
});
