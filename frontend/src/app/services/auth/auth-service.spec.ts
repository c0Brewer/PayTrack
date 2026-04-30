import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';
import { Role, UserDto } from '../../types/exporter';

import { AuthService } from './auth-service';

describe('AuthService', () => {
  let service: AuthService;
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  function createValidJwt(): string {
    const payload = {
      exp: Math.floor(Date.now() / 1000) + 3600, // expires in 1 hour
    };

    return `header.${btoa(JSON.stringify(payload))}.signature`;
  }

  function createInvalidJwt(): string {
    const payload = {
      exp: Math.floor(Date.now() / 1000) - 3600, // expired 1 hour ago
    };

    return `header.${btoa(JSON.stringify(payload))}.signature`;
  }

  beforeEach(() => {
    routerMock = {
      navigate: vi.fn(),
    };

    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [AuthService, { provide: Router, useValue: routerMock }],
    });

    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('should call API and return data for handleGoogleCallback', async () => {
    const apiResponse = { jwtToken: 'token123' };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: apiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await firstValueFrom(service.handleGoogleCallback('google-code'));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/auth/google', {
      params: {},
      body: { code: 'google-code' },
    });

    expect(result).toEqual(apiResponse);
  });

  it('should call API and return error if error occurs', async () => {
    const error = {
      detail: 'An error occured',
    };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: null,
      error: error,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(firstValueFrom(service.handleGoogleCallback('google-id'))).rejects.toThrow(
      error.detail,
    );
  });

  it('should call API and return default error message if error occurs and no message is set', async () => {
    const error = {};

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: null,
      error: error,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await expect(firstValueFrom(service.handleGoogleCallback('google-id'))).rejects.toThrow(
      'Unexpected Error',
    );
  });

  it('should return is logged in true after store token', () => {
    const currentUserApiResponse: UserDto = {
      id: 123,
      name: 'name',
      email: 'email',
      isActive: true,
      team: { id: -1, name: '123' },
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      bankAccounts: [],
      bankInformationSkipped: true,
      hasBankInformation: true,
    };

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: currentUserApiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    service.storeToken(createValidJwt());

    const isLoggedIn = service.isLoggedIn();

    expect(isLoggedIn).toEqual(true);
  });

  it('should return is logged in false after logout has been called', () => {
    const currentUserApiResponse: UserDto = {
      id: 123,
      name: 'name',
      email: 'email',
      isActive: true,
      team: { id: -1, name: '123' },
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      bankAccounts: [],
      bankInformationSkipped: true,
      hasBankInformation: true,
    };

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: currentUserApiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    service.storeToken(createValidJwt());

    let isLoggedIn = service.isLoggedIn();
    expect(isLoggedIn).toEqual(true);

    service.logout();

    isLoggedIn = service.isLoggedIn();
    expect(isLoggedIn).toEqual(false);
  });

  it('should return invalid for invalid token', () => {
    localStorage.setItem('jwt', createInvalidJwt());

    const isLoggedIn = service.isLoggedIn();

    expect(isLoggedIn).toEqual(false);
  });

  it('refresh user should call backend', async () => {
    const currentUserApiResponse: UserDto = {
      id: 123,
      name: 'name',
      email: 'email',
      isActive: true,
      team: { id: -1, name: '123' },
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      bankAccounts: [],
      bankInformationSkipped: true,
      hasBankInformation: true,
    };

    vi.spyOn(client, 'GET').mockResolvedValue({
      data: currentUserApiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    await service.refreshUser();

    const user = await firstValueFrom(service.getCurrentUser());

    expect(user).toEqual(currentUserApiResponse);
  });

  it('fetchAndStoreUser should throw error when API returns error', async () => {
    const apiError = { detail: 'Fetch error' };
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.spyOn(client, 'GET').mockResolvedValue({ data: null, error: apiError } as any);

    await expect(service.fetchAndStoreUser()).rejects.toThrow(apiError.detail);
  });

  it('initCurrentUser should call logout and show error if fetch fails', async () => {
    const apiError = { detail: 'Fetch error' };
    vi.spyOn(service, 'fetchAndStoreUser').mockRejectedValue(apiError);
    const logoutSpy = vi.spyOn(service, 'logout');
    const notifSpy = vi.spyOn(service['notificationService'], 'showError');

    await service['initCurrentUser']();

    expect(logoutSpy).toHaveBeenCalledOnce();
    expect(notifSpy).toHaveBeenCalledOnce();
    expect(notifSpy).toHaveBeenCalledWith(expect.stringContaining('Error while loading User'));
  });

  it('checkExpiryOnStartup should logout if token expired', () => {
    const expiredToken = createInvalidJwt();
    localStorage.setItem('jwt', expiredToken);
    const logoutSpy = vi.spyOn(service, 'logout');

    service['checkExpiryOnStartup']();

    expect(logoutSpy).toHaveBeenCalledOnce();
  });

  it('loadGoogleScript should append script if google is undefined', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    delete (globalThis.window as any).google;
    const scriptAppendSpy = vi.spyOn(document.body, 'appendChild');

    const loadPromise = service.loadGoogleScript();
    const script = scriptAppendSpy.mock.calls[0][0] as HTMLScriptElement;
    script.onload?.(new Event('load'));

    await loadPromise;

    expect(script.src).toContain('https://accounts.google.com/gsi/client');
  });

  it('loadGoogleScript should resolve immediately if google already exists', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (globalThis.window as any).google = {};
    const scriptAppendSpy = vi.spyOn(document.body, 'appendChild');

    await service.loadGoogleScript();

    expect(scriptAppendSpy).not.toHaveBeenCalled();
  });

  it('needsBankInformation should return true only if user has neither bank info nor skip flag', () => {
    expect(
      service.needsBankInformation({
        id: 1,
        name: 'name',
        email: 'email',
        isActive: true,
        team: { id: -1, name: '123' },
        role: Role.REGULAR_USER,
        profilePictureUrl: '',
        hasBankInformation: false,
        bankInformationSkipped: false,
        bankAccounts: [],
      }),
    ).toBe(true);

    expect(
      service.needsBankInformation({
        id: 1,
        name: 'name',
        email: 'email',
        isActive: true,
        team: { id: -1, name: '123' },
        role: Role.REGULAR_USER,
        profilePictureUrl: '',
        hasBankInformation: true,
        bankInformationSkipped: false,
        bankAccounts: [],
      }),
    ).toBe(false);

    expect(service.needsBankInformation(null)).toBe(false);
  });

  it('skipBankInformation should call backend and update current user', async () => {
    const currentUserApiResponse: UserDto = {
      id: 123,
      name: 'name',
      email: 'email',
      isActive: true,
      team: { id: -1, name: '123' },
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
      hasBankInformation: false,
      bankInformationSkipped: true,
      bankAccounts: [],
    };

    vi.spyOn(client, 'POST').mockResolvedValue({
      data: currentUserApiResponse,
      error: null,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const result = await service.skipBankInformation();

    expect(client.POST).toHaveBeenCalledWith('/api/v1/bankaccount/onboarding/skip', {
      params: {},
    });
    expect(result).toEqual(currentUserApiResponse);
    expect(await firstValueFrom(service.getCurrentUser())).toEqual(currentUserApiResponse);
  });
});
