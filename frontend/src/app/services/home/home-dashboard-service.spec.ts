import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../auth/auth-service';

import { HomeDashboardService } from './home-dashboard-service';

describe('HomeDashboardService', () => {
  let service: HomeDashboardService;

  const authServiceMock = {
    getToken: vi.fn(),
  };

  const originalFetch = globalThis.fetch;

  beforeEach(() => {
    authServiceMock.getToken.mockReset();
    globalThis.fetch = vi.fn();

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
    vi.restoreAllMocks();
  });

  it('should request the dashboard with the auth token', async () => {
    authServiceMock.getToken.mockReturnValue('jwt-token');
    vi.mocked(globalThis.fetch).mockResolvedValue({
      ok: true,
      json: async () => ({
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
      }),
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
  });

  it('should request the dashboard without authorization header when no token exists', async () => {
    authServiceMock.getToken.mockReturnValue(null);
    vi.mocked(globalThis.fetch).mockResolvedValue({
      ok: true,
      json: async () => ({
        user: { id: 1, name: 'Alex', role: 0 },
        invoices: {
          openCount: 0,
          submittedCount: 0,
          paidCount: 0,
          openAmount: 0,
          lastPaidAt: null,
          totalRecentCount: 0,
          recent: [],
        },
        paymentRequests: {
          openCount: 0,
          submittedCount: 0,
          paidCount: 0,
          openAmount: 0,
          lastPaidAt: null,
          totalRecentCount: 0,
          recent: [],
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
});
