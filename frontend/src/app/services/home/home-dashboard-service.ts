import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Role, TransactionStatus } from '../../types/exporter';
import { AuthService } from '../auth/auth-service';
import { withOfflineReadFallback } from '../offline/offline-utils';

export interface HomeDashboardRecentItem {
  id: number;
  amount: number;
  status: TransactionStatus;
  createdAt: string | null;
  paidAt: string | null;
  reference: string | null;
  purposeOfPayment: string | null;
  teamName: string | null;
  userName: string | null;
}

export interface HomeDashboardSection {
  openCount: number;
  submittedCount: number;
  paidCount: number;
  openAmount: number;
  lastPaidAt: string | null;
  recent: HomeDashboardRecentItem[];
}

export interface HomeDashboardActions {
  missingBankAccount: boolean;
  bankInformationSkipped: boolean;
  needsAttentionCount: number;
}

export interface HomeDashboardUser {
  id: number;
  name: string;
  role: Role;
}

export interface HomeDashboardDto {
  user: HomeDashboardUser;
  invoices: HomeDashboardSection;
  paymentRequests: HomeDashboardSection;
  actions: HomeDashboardActions;
}

@Injectable({
  providedIn: 'root',
})
export class HomeDashboardService {
  constructor(private readonly authService: AuthService) {}

  private getApiUrl(path: string): string {
    return environment.apiBaseUrl ? new URL(path, environment.apiBaseUrl).toString() : path;
  }

  public getHomeDashboard(): Observable<HomeDashboardDto> {
    const token = this.authService.getToken();

    const promise = fetch(this.getApiUrl('/api/v1/dashboard/home'), {
      method: 'GET',
      headers: {
        Accept: 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    }).then(async (res) => {
      if (!res.ok) {
        const error = (await res.json().catch(() => null)) as { detail?: string } | null;
        throw new Error(error?.detail ?? 'Unexpected Error');
      }

      return (await res.json()) as HomeDashboardDto;
    });

    return from(withOfflineReadFallback(promise, 'Error while loading dashboard'));
  }
}
