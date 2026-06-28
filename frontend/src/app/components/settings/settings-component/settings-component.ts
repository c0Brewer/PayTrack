import { AsyncPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { Role, UserDto } from '../../../types/exporter';

import { AdminSettingsSettingsPageComponent } from './settings-pages/admin-settings-settings-page/admin-settings-settings-page';
import { BankAccountsSettingsPageComponent } from './settings-pages/bank-accounts-settings-page/bank-accounts-settings-page';
import { NotificationsSettingsPageComponent } from './settings-pages/notifications-settings-page/notifications-settings-page';
import { ProfileSettingsPageComponent } from './settings-pages/profile-settings-page/profile-settings-page';

type SettingsTab = {
  id: string;
  label: string;
  icon: string;
  adminOnly?: boolean;
};

@Component({
  selector: 'app-settings-component',
  imports: [
    AdminSettingsSettingsPageComponent,
    BankAccountsSettingsPageComponent,
    NotificationsSettingsPageComponent,
    ProfileSettingsPageComponent,
    RouterLink,
    AsyncPipe,
  ],
  templateUrl: './settings-component.html',
  styleUrl: './settings-component.scss',
})
export class SettingsComponent implements OnInit, OnDestroy {
  protected readonly tabs: SettingsTab[] = [
    {
      id: 'profile',
      label: 'Profile',
      icon: 'account_circle',
    },
    {
      id: 'bank-accounts',
      label: 'Bank Accounts',
      icon: 'credit_card_gear',
    },
    {
      id: 'notifications',
      label: 'Notifications',
      icon: 'edit_notifications',
    },
    {
      id: 'admin-settings',
      label: 'Administration',
      icon: 'admin_panel_settings',
      adminOnly: true,
    },
  ];

  private readonly activeTabId = signal(this.tabs[0].id);
  protected readonly activeTab = computed(() => this.activeTabId());
  private fragmentSubscription?: Subscription;

  constructor(
    private readonly route: ActivatedRoute,
    public readonly authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.fragmentSubscription = this.route.fragment.subscribe((fragment) => {
      const nextTabId =
        fragment != null && this.tabs.some((tab) => tab.id === fragment)
          ? fragment
          : this.tabs[0].id;
      this.activeTabId.set(nextTabId);
    });
  }

  ngOnDestroy(): void {
    this.fragmentSubscription?.unsubscribe();
  }

  protected isActiveTab(tabId: string): boolean {
    return this.activeTabId() === tabId;
  }

  protected filteredTabs(user: UserDto | null): SettingsTab[] {
    return this.tabs.filter((tab) => !tab.adminOnly || user?.role === Role.ADMIN);
  }

  protected hasNoBankAccounts(user: UserDto | null): boolean {
    return !!user && (user.bankAccounts?.length ?? 0) === 0;
  }
}
