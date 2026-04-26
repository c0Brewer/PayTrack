import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { AuthService } from '../../../services/auth/auth-service';
import { GoogleAuthResponseDto } from '../../../types/exporter';

import { LoginComponent } from './login-component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  let authServiceMock: {
    handleGoogleCallback: ReturnType<typeof vi.fn>;
    loadGoogleScript: ReturnType<typeof vi.fn>;
    storeToken: ReturnType<typeof vi.fn>;
  };

  let routerMock: {
    navigate: ReturnType<typeof vi.fn>;
  };

  const mockGoogleCallbackResponse = {
    credential: 'abc',
  };

  const mockJwtCallbackResponse: GoogleAuthResponseDto = {
    jwtToken: '123',
  };

  beforeEach(async () => {
    authServiceMock = {
      handleGoogleCallback: vi.fn().mockReturnValue(of(mockJwtCallbackResponse)),
      loadGoogleScript: vi.fn(),
      storeToken: vi.fn(),
    };

    routerMock = {
      navigate: vi.fn(),
    };

    // mock Google API
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (window as any).google = {
      accounts: {
        id: {
          initialize: vi.fn(),
          renderButton: vi.fn(),
        },
      },
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges(); // triggers ngOnInit
    expect(component).toBeTruthy();
  });

  it('should redirect to home ("") on successful login', () => {
    authServiceMock.loadGoogleScript.mockReturnValue(of(null));
    authServiceMock.handleGoogleCallback.mockReturnValue(of(mockJwtCallbackResponse));

    component.handleCredentialResponse(mockGoogleCallbackResponse);

    expect(authServiceMock.handleGoogleCallback).toHaveBeenCalledWith(
      mockGoogleCallbackResponse.credential,
    );
    expect(authServiceMock.storeToken).toHaveBeenCalledWith(mockJwtCallbackResponse.jwtToken);
    expect(routerMock.navigate).toHaveBeenCalledWith(['']);
  });
});
