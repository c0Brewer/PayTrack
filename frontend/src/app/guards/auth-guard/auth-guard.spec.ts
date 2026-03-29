import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { vi } from 'vitest';

import { AuthService } from '../../services/auth/auth-service';

import { authGuard } from './auth-guard';

describe('authGuard', () => {
  let authServiceMock: { isLoggedIn: ReturnType<typeof vi.fn> };
  let routerMock: { createUrlTree: ReturnType<typeof vi.fn> };

  // eslint-disable-next-line @typescript-eslint/explicit-function-return-type, @typescript-eslint/no-explicit-any
  const executeGuard = () => TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

  beforeEach(() => {
    authServiceMock = {
      isLoggedIn: vi.fn(),
    };

    routerMock = {
      createUrlTree: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should allow navigation when user is logged in', () => {
    authServiceMock.isLoggedIn.mockReturnValue(true);

    const result = executeGuard();

    expect(result).toBe(true);
    expect(authServiceMock.isLoggedIn).toHaveBeenCalled();
  });

  it('should redirect to /login when user is not logged in', () => {
    const urlTree = {} as UrlTree;

    authServiceMock.isLoggedIn.mockReturnValue(false);
    routerMock.createUrlTree.mockReturnValue(urlTree);

    const result = executeGuard();

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/login']);
    expect(result).toBe(urlTree);
  });
});
