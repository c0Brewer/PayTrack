import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

import { client } from '../../client';

import { AuthService } from './auth-service';

describe('AuthService', () => {
  let service: AuthService;
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

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
    service.storeToken('123');

    const isLoggedIn = service.isLoggedIn();

    expect(isLoggedIn).toEqual(true);
  });

  it('should return is logged in false after logout has been called', () => {
    service.storeToken('123');

    let isLoggedIn = service.isLoggedIn();
    expect(isLoggedIn).toEqual(true);

    service.logout();

    isLoggedIn = service.isLoggedIn();
    expect(isLoggedIn).toEqual(false);
  });
});
