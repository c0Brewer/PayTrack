import { Injectable, computed, signal } from '@angular/core';

import { NotificationService } from '../notification/notification-service';

import { OFFLINE_READ_MESSAGE, OFFLINE_WRITE_MESSAGE, isBrowserOnline } from './offline-utils';

@Injectable({
  providedIn: 'root',
})
export class OfflineService {
  readonly isOffline = signal(!isBrowserOnline());
  readonly bannerMessage = computed(() =>
    this.isOffline()
      ? 'Offline mode: previously loaded pages remain available from cache. Live data cannot be refreshed, and most write actions are disabled. Invoice submissions can still be saved locally for later sync.'
      : '',
  );

  private initialized = false;

  constructor(private readonly notificationService: NotificationService) {
    this.init();
  }

  init(): void {
    if (this.initialized || typeof window === 'undefined') {
      return;
    }

    this.initialized = true;

    window.addEventListener('online', () => {
      this.isOffline.set(false);
      this.notificationService.showSuccess('Connection restored. Live data can refresh again.');
    });

    window.addEventListener('offline', () => {
      this.isOffline.set(true);
      this.notificationService.showError(OFFLINE_READ_MESSAGE, 5000);
    });
  }

  get offlineReadMessage(): string {
    return OFFLINE_READ_MESSAGE;
  }

  get offlineWriteMessage(): string {
    return OFFLINE_WRITE_MESSAGE;
  }
}
