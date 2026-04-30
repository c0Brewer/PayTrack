import { AsyncPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';

import { BankAccountsSettingsPageComponent } from './settings-pages/bank-accounts-settings-page/bank-accounts-settings-page';
import { NotificationsSettingsPageComponent } from './settings-pages/notifications-settings-page/notifications-settings-page';
import { ProfileSettingsPageComponent } from './settings-pages/profile-settings-page/profile-settings-page';
import { SecuritySettingsPageComponent } from './settings-pages/security-settings-page/security-settings-page';

type SettingsTab = {
  id: string;
  label: string;
  icon: string;
};

@Component({
  selector: 'app-settings-component',
  imports: [
    BankAccountsSettingsPageComponent,
    NotificationsSettingsPageComponent,
    ProfileSettingsPageComponent,
    RouterLink,
    SecuritySettingsPageComponent,
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
      id: 'security',
      label: 'Security',
      icon: 'security',
    },
    {
      id: 'notifications',
      label: 'Notifications',
      icon: 'edit_notifications',
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
}
