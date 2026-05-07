import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';

import { NavbarComponent } from './navbar-component';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

  const authServiceMock = {
    logout: vi.fn(),
    loggedIn$: of(false),
    currentUser$: of(null),
  };

  const paymentServiceMock = {
    getPaymentRequestsByUser: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: PaymentRequestByUserService, useValue: paymentServiceMock },
      ],
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
});
