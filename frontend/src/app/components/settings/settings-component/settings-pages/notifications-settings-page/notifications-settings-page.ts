import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input } from '@angular/core';

import { NotificationService } from '../../../../../services/notification/notification-service';
import { PushNotificationService } from '../../../../../services/push-notification/push-notification-service';
import { UserDto } from '../../../../../types/exporter';
import { BoxComponent } from '../../../../general/boxes/box-component/box-component';

@Component({
  selector: 'app-notifications-settings-page',
  imports: [BoxComponent, DatePipe],
  templateUrl: './notifications-settings-page.html',
  styleUrl: './notifications-settings-page.scss',
})
export class NotificationsSettingsPageComponent implements OnInit {
  user = input<UserDto | null>(null);

  private readonly pushNotifications = inject(PushNotificationService);
  private readonly notifications = inject(NotificationService);

  protected readonly enabled = this.pushNotifications.enabled;
  protected readonly loading = this.pushNotifications.loading;
  protected readonly availability = this.pushNotifications.availability;
  protected readonly devices = this.pushNotifications.devices;
  protected readonly canToggle = computed(
    () => this.availability() === 'available' && !this.loading(),
  );
  protected readonly statusText = computed(() => {
    switch (this.availability()) {
      case 'available':
        return this.enabled()
          ? 'Push notifications are enabled for invoice and payment request updates.'
          : 'Push notifications are available for this browser.';
      case 'unsupported-browser':
        return 'Push notifications are not supported by this browser or operating system.';
      case 'unsupported-context':
        return 'Push notifications require HTTPS or localhost.';
      case 'service-worker-disabled':
        return 'Push notifications are only available when the installed service worker is active.';
      case 'server-not-configured':
        return 'Push notifications are not configured for this PayTrack deployment.';
      case 'permission-denied':
        return 'Notifications are blocked by your browser or operating system settings.';
      default:
        return 'Checking push notification support.';
    }
  });

  ngOnInit(): void {
    this.pushNotifications.loadConfig().catch(() => {
      this.notifications.showError('Could not load push notification settings.');
    });
  }

  protected onToggle(event: Event): void {
    const target = event.target as HTMLInputElement;
    const checked = target.checked;
    const action = checked ? this.pushNotifications.enable() : this.pushNotifications.disable();

    action
      .then(() => {
        this.notifications.showSuccess(
          checked ? 'Push notifications enabled.' : 'Push notifications disabled.',
        );
      })
      .catch((error: unknown) => {
        const message = error instanceof Error ? error.message : 'Could not update push notification settings.';
        this.notifications.showError(message);
      })
      .finally(() => {
        target.checked = this.enabled();
      });
  }
}
