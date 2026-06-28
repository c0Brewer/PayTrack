import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../auth/auth-service';
import { OFFLINE_READ_MESSAGE } from '../offline/offline-utils';

import { HomeDashboardService } from './home-dashboard-service';

describe('HomeDashboardService', () => {
  let service: HomeDashboardService;

  const authServiceMock = {
    getToken: vi.fn(),
  };

  const originalFetch = globalThis.fetch;
  const originalNavigator = globalThis.navigator;

  const dashboardResponse = {
    user: { id: 1, name: 'Alex', role: 0 },
    invoices: {
      openCount: 1,
      submittedCount: 2,
      paidCount: 3,
      openAmount: 50,
      lastPaidAt: null,
      totalRecentCount: 1,
      recent: [],
    },
    paymentRequests: {
      openCount: 4,
      submittedCount: 5,
      paidCount: 6,
      openAmount: 75,
      lastPaidAt: null,
      totalRecentCount: 8,
      recent: [],
    },
    actions: {
      missingBankAccount: false,
      bankInformationSkipped: false,
      needsAttentionCount: 1,
    },
  };

  beforeEach(() => {
    authServiceMock.getToken.mockReset();
    globalThis.fetch = vi.fn();
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        HomeDashboardService,
        { provide: AuthService, useValue: authServiceMock },
      ],
    });

    service = TestBed.inject(HomeDashboardService);
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
    Object.defineProperty(globalThis, 'navigator', {
      value: originalNavigator,
      configurable: true,
    });
    vi.restoreAllMocks();
  });

  it('should request the dashboard with the auth token', async () => {
    authServiceMock.getToken.mockReturnValue('jwt-token');
    vi.mocked(globalThis.fetch).mockResolvedValue({
      ok: true,
      json: async () => dashboardResponse,
    } as Response);

    const result = await firstValueFrom(service.getHomeDashboard());

    expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5154/api/v1/dashboard/home', {
      method: 'GET',
      headers: {
        Accept: 'application/json',
        Authorization: 'Bearer jwt-token',
      },
    });
    expect(result.user.name).toBe('Alex');
    expect(result.actions.needsAttentionCount).toBe(1);
    expect(localStorage.getItem('home-dashboard-cache')).toBe(JSON.stringify(dashboardResponse));
  });

  it('should request the dashboard without authorization header when no token exists', async () => {
    authServiceMock.getToken.mockReturnValue(null);
    vi.mocked(globalThis.fetch).mockResolvedValue({
      ok: true,
      json: async () => ({
        ...dashboardResponse,
        invoices: {
          ...dashboardResponse.invoices,
          openCount: 0,
          submittedCount: 0,
          paidCount: 0,
          openAmount: 0,
          totalRecentCount: 0,
        },
        paymentRequests: {
          ...dashboardResponse.paymentRequests,
          openCount: 0,
          submittedCount: 0,
          paidCount: 0,
          openAmount: 0,
          totalRecentCount: 0,
        },
        actions: {
          missingBankAccount: false,
          bankInformationSkipped: false,
          needsAttentionCount: 0,
        },
      }),
    } as Response);

    await firstValueFrom(service.getHomeDashboard());

    expect(globalThis.fetch).toHaveBeenCalledWith('http://localhost:5154/api/v1/dashboard/home', {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });
  });

  it('should surface backend errors', async () => {
    authServiceMock.getToken.mockReturnValue('jwt-token');
    vi.mocked(globalThis.fetch).mockResolvedValue({
      ok: false,
      json: async () => ({ detail: 'Dashboard failed' }),
    } as Response);

    await expect(firstValueFrom(service.getHomeDashboard())).rejects.toThrow('Dashboard failed');
  });

  it('should return cached dashboard data while offline', async () => {
    authServiceMock.getToken.mockReturnValue('jwt-token');
    localStorage.setItem('home-dashboard-cache', JSON.stringify(dashboardResponse));
    Object.defineProperty(globalThis, 'navigator', {
      value: { onLine: false },
      configurable: true,
    });
    vi.mocked(globalThis.fetch).mockRejectedValue(new TypeError('Failed to fetch'));

    const result = await firstValueFrom(service.getHomeDashboard());

    expect(result).toEqual(dashboardResponse);
  });

  it('should show offline read message when offline and no cache exists', async () => {
    authServiceMock.getToken.mockReturnValue('jwt-token');
    Object.defineProperty(globalThis, 'navigator', {
      value: { onLine: false },
      configurable: true,
    });
    vi.mocked(globalThis.fetch).mockRejectedValue(new TypeError('Failed to fetch'));

    await expect(firstValueFrom(service.getHomeDashboard())).rejects.toThrow(OFFLINE_READ_MESSAGE);
  });
});
