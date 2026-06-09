import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  GuardResult,
  MaybeAsync,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { vi, describe, it, expect, beforeEach } from 'vitest';

import { AuthService } from '../../services/auth/auth-service';
import { Role, UserDto } from '../../types/exporter';

import { roleGuard } from './role-guard';

describe('roleGuard', () => {
  let authServiceMock: { fetchAndStoreUser: ReturnType<typeof vi.fn> };
  let routerMock: { createUrlTree: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authServiceMock = {
      fetchAndStoreUser: vi.fn(),
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

  function runGuard(requiredRole: Role): MaybeAsync<GuardResult> {
    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;

    return TestBed.runInInjectionContext(() => roleGuard(requiredRole)(route, state));
  }

  it('should allow access if user has required role', async () => {
    const user = { role: Role.ADMIN, isActive: true } as UserDto;

    authServiceMock.fetchAndStoreUser.mockResolvedValue(user);

    const result = await runGuard(Role.TEAM_LEAD);

    expect(result).toBe(true);
  });

  it('should redirect to login if user is null', async () => {
    const urlTree = {} as UrlTree;

    authServiceMock.fetchAndStoreUser.mockResolvedValue(null);
    routerMock.createUrlTree.mockReturnValue(urlTree);

    const result = await runGuard(Role.REGULAR_USER);

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/login']);
    expect(result).toBe(urlTree);
  });

  it('should redirect to unauthorized if user is inactive', async () => {
    const urlTree = {} as UrlTree;

    const user = { role: Role.ADMIN, isActive: false } as UserDto;

    authServiceMock.fetchAndStoreUser.mockResolvedValue(user);
    routerMock.createUrlTree.mockReturnValue(urlTree);

    const result = await runGuard(Role.ADMIN);

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/unauthorized']);
    expect(result).toBe(urlTree);
  });

  it('should redirect to unauthorized if role is insufficient', async () => {
    const urlTree = {} as UrlTree;

    const user = { role: Role.REGULAR_USER } as UserDto;

    authServiceMock.fetchAndStoreUser.mockResolvedValue(user);
    routerMock.createUrlTree.mockReturnValue(urlTree);

    const result = await runGuard(Role.ADMIN);

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/unauthorized']);
    expect(result).toBe(urlTree);
  });
});
