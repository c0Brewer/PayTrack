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

    const result = await firstValueFrom(service.handleGoogleCallback('google-id'));

    expect(client.POST).toHaveBeenCalledWith('/api/v1/auth/google', {
      params: {},
      body: { idToken: 'google-id' },
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
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
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
      role: Role.REGULAR_USER,
      profilePictureUrl: '',
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

  it('refresh user should call backend', () => {
    localStorage.setItem('jwt', createInvalidJwt());

    const isLoggedIn = service.isLoggedIn();

    expect(isLoggedIn).toEqual(false);
  });
});
