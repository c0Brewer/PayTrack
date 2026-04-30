import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';

import { AuthService } from '../../../services/auth/auth-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { GoogleAuthResponseDto, Role, UserDto } from '../../../types/exporter';

import { LoginComponent } from './login-component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  let authServiceMock: {
    handleGoogleCallback: ReturnType<typeof vi.fn>;
    loadGoogleScript: ReturnType<typeof vi.fn>;
    storeToken: ReturnType<typeof vi.fn>;
    needsBankInformation: ReturnType<typeof vi.fn>;
  };

  let routerMock: {
    navigate: ReturnType<typeof vi.fn>;
  };

  let notificationMock: {
    showError: ReturnType<typeof vi.fn>;
  };

  const mockGoogleResponse = {
    code: 'abc',
  };

  const mockJwtResponse: GoogleAuthResponseDto = {
    jwtToken: '123',
  };

  const mockUser: UserDto = {
    id: 1,
    name: 'name',
    email: 'email',
    isActive: true,
    team: { id: -1, name: 'team' },
    role: Role.REGULAR_USER,
    profilePictureUrl: '',
    bankInformationSkipped: true,
    hasBankInformation: true,
    bankAccounts: [],
  };

  const requestCodeMock = vi.fn();

  beforeEach(async () => {
    authServiceMock = {
      handleGoogleCallback: vi.fn(),
      loadGoogleScript: vi.fn().mockResolvedValue(undefined),
      storeToken: vi.fn().mockResolvedValue(mockUser),
      needsBankInformation: vi.fn().mockReturnValue(false),
    };

    routerMock = {
      navigate: vi.fn(),
    };

    notificationMock = {
      showError: vi.fn(),
    };

    // ✅ Correct Google API mock
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (window as any).google = {
      accounts: {
        oauth2: {
          initCodeClient: vi.fn().mockReturnValue({
            requestCode: requestCodeMock,
          }),
        },
      },
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: NotificationService, useValue: notificationMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  // -------------------------
  // BASIC
  // -------------------------
  it('should create', async () => {
    await component.ngAfterViewInit();
    expect(component).toBeTruthy();
  });

  // -------------------------
  // INIT GOOGLE CLIENT
  // -------------------------
  it('should initialize google client on view init', async () => {
    await component.ngAfterViewInit();

    expect(authServiceMock.loadGoogleScript).toHaveBeenCalled();
    expect(window.google?.accounts.oauth2.initCodeClient).toHaveBeenCalled();
  });

  it('should show error if google not available', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (window as any).google = undefined;

    await component.ngAfterViewInit();

    expect(notificationMock.showError).toHaveBeenCalledWith('Google login is not ready yet.');
  });

  // -------------------------
  // SIGN IN
  // -------------------------
  it('should call requestCode on signInWithGoogle', async () => {
    await component.ngAfterViewInit();

    component.signInWithGoogle();

    expect(requestCodeMock).toHaveBeenCalled();
  });

  it('should show error if signIn called before init', () => {
    component.signInWithGoogle();

    expect(notificationMock.showError).toHaveBeenCalledWith('Google login is not ready yet.');
  });

  // -------------------------
  // HANDLE RESPONSE SUCCESS
  // -------------------------
  it('should handle google login success', () => {
    authServiceMock.handleGoogleCallback.mockReturnValue({
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      subscribe: ({ next }: any) => next(mockJwtResponse),
    });

    component.handleGoogleCodeResponse(mockGoogleResponse);

    expect(authServiceMock.handleGoogleCallback).toHaveBeenCalledWith('abc');
    expect(authServiceMock.storeToken).toHaveBeenCalledWith('123');
  });

  // -------------------------
  // HANDLE RESPONSE ERROR
  // -------------------------
  it('should show error if no code provided', () => {
    component.handleGoogleCodeResponse({});

    expect(notificationMock.showError).toHaveBeenCalledWith('Google login failed.');
  });

  it('should handle backend error', () => {
    authServiceMock.handleGoogleCallback.mockReturnValue({
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      subscribe: ({ error }: any) => error(new Error('Login failed')),
    });

    component.handleGoogleCodeResponse(mockGoogleResponse);

    expect(notificationMock.showError).toHaveBeenCalledWith('Login failed');
  });

  // it('should redirect to home ("") on successful login without bank onboarding', async () => {
  //   authServiceMock.loadGoogleScript.mockReturnValue(of(null));
  //   authServiceMock.handleGoogleCallback.mockReturnValue(of(mockJwtResponse));
  //   authServiceMock.storeToken.mockResolvedValue(mockUser);
  //   authServiceMock.needsBankInformation.mockReturnValue(false);
  //
  //   await Promise.resolve();
  //   expect(authServiceMock.handleGoogleCallback).toHaveBeenCalledWith(
  //     mockGoogleCallbackResponse.credential,
  //   );
  //   expect(authServiceMock.storeToken).toHaveBeenCalledWith(mockJwtCallbackResponse.jwtToken);
  //   expect(routerMock.navigate).toHaveBeenCalledWith(['']);
  // });
  //
  // it('should redirect to bank-information on successful login when onboarding is needed', async () => {
  //   authServiceMock.handleGoogleCallback.mockReturnValue(of(mockJwtCallbackResponse));
  //   authServiceMock.storeToken.mockResolvedValue(mockUser);
  //   authServiceMock.needsBankInformation.mockReturnValue(true);
  //
  //   component.handleCredentialResponse(mockGoogleCallbackResponse);
  //
  //   await Promise.resolve();
  //   expect(routerMock.navigate).toHaveBeenCalledWith(['bank-information']);
  // });
});
