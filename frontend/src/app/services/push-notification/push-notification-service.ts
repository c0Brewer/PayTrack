import { Injectable, signal } from '@angular/core';

import { environment } from '../../../environments/environment';

export type PushNotificationAvailability =
  | 'checking'
  | 'available'
  | 'unsupported-browser'
  | 'unsupported-context'
  | 'service-worker-disabled'
  | 'server-not-configured'
  | 'permission-denied';

interface PushNotificationConfig {
  isConfigured: boolean;
  vapidPublicKey?: string | null;
  enabled: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class PushNotificationService {
  readonly availability = signal<PushNotificationAvailability>('checking');
  readonly enabled = signal(false);
  readonly loading = signal(false);

  private vapidPublicKey: string | null = null;

  async loadConfig(): Promise<void> {
    this.loading.set(true);

    try {
      const localAvailability = await this.getLocalAvailability();
      if (localAvailability !== 'available') {
        this.availability.set(localAvailability);
        this.enabled.set(false);
        return;
      }

      const config = await this.request<PushNotificationConfig>('/api/v1/notification/push/config');
      if (!config.isConfigured || !config.vapidPublicKey) {
        this.availability.set('server-not-configured');
        this.enabled.set(false);
        return;
      }

      if (Notification.permission === 'denied') {
        this.availability.set('permission-denied');
        this.enabled.set(false);
        return;
      }

      this.vapidPublicKey = config.vapidPublicKey;
      this.enabled.set(config.enabled);
      this.availability.set('available');
    } finally {
      this.loading.set(false);
    }
  }

  async enable(): Promise<void> {
    if (!this.vapidPublicKey) {
      await this.loadConfig();
    }

    if (this.availability() !== 'available' || !this.vapidPublicKey) {
      return;
    }

    this.loading.set(true);

    try {
      const registration = await this.ensureServiceWorkerRegistration();
      const subscription =
        (await registration.pushManager.getSubscription()) ??
        (await registration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: this.urlBase64ToUint8Array(this.vapidPublicKey),
        }));
      const json = subscription.toJSON();

      await this.request<void>('/api/v1/notification/push/subscribe', {
        method: 'POST',
        body: JSON.stringify({
          endpoint: json.endpoint,
          p256dh: json.keys?.['p256dh'],
          auth: json.keys?.['auth'],
        }),
      });

      this.enabled.set(true);
    } catch {
      if (Notification.permission === 'denied') {
        this.availability.set('permission-denied');
      }

      this.enabled.set(false);
      throw new Error('Push notifications could not be enabled.');
    } finally {
      this.loading.set(false);
    }
  }

  async disable(): Promise<void> {
    this.loading.set(true);

    try {
      const registration = await navigator.serviceWorker.getRegistration();
      if (!registration) {
        this.enabled.set(false);
        return;
      }

      const subscription = await registration.pushManager.getSubscription();

      if (subscription) {
        await this.request<void>('/api/v1/notification/push/unsubscribe', {
          method: 'POST',
          body: JSON.stringify({
            endpoint: subscription.endpoint,
          }),
        });
        await subscription.unsubscribe();
      }

      this.enabled.set(false);
    } finally {
      this.loading.set(false);
    }
  }

  private async getLocalAvailability(): Promise<PushNotificationAvailability> {
    if (!globalThis.isSecureContext) {
      return 'unsupported-context';
    }

    if (
      !('Notification' in window) ||
      !('serviceWorker' in navigator) ||
      !('PushManager' in window)
    ) {
      return 'unsupported-browser';
    }

    if (Notification.permission === 'denied') {
      return 'permission-denied';
    }

    try {
      await this.ensureServiceWorkerRegistration();
    } catch {
      return 'service-worker-disabled';
    }

    return 'available';
  }

  private async ensureServiceWorkerRegistration(): Promise<ServiceWorkerRegistration> {
    const existingRegistration = await navigator.serviceWorker.getRegistration();
    if (
      existingRegistration?.active ||
      existingRegistration?.installing ||
      existingRegistration?.waiting
    ) {
      return existingRegistration;
    }

    const workerResponse = await fetch('/ngsw-worker.js', {
      cache: 'no-store',
    });
    const contentType = workerResponse.headers.get('content-type') ?? '';

    if (!workerResponse.ok || !contentType.includes('javascript')) {
      throw new Error('Angular service worker is not available.');
    }

    await navigator.serviceWorker.register('/ngsw-worker.js');
    return await this.waitForServiceWorkerReady();
  }

  private async waitForServiceWorkerReady(): Promise<ServiceWorkerRegistration> {
    return await Promise.race([
      navigator.serviceWorker.ready,
      new Promise<never>((_, reject) => {
        window.setTimeout(() => reject(new Error('Service worker registration timed out.')), 5000);
      }),
    ]);
  }

  private urlBase64ToUint8Array(value: string): Uint8Array<ArrayBuffer> {
    const padding = '='.repeat((4 - (value.length % 4)) % 4);
    const base64 = `${value}${padding}`.replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const output = new Uint8Array(new ArrayBuffer(rawData.length));

    for (let i = 0; i < rawData.length; i++) {
      output[i] = rawData.charCodeAt(i);
    }

    return output;
  }

  private async request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const token = localStorage.getItem('jwt');
    const response = await fetch(`${environment.apiBaseUrl}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...init.headers,
      },
    });

    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`);
    }

    if (response.status === 204) {
      return undefined as T;
    }

    const text = await response.text();
    return text ? (JSON.parse(text) as T) : (undefined as T);
  }
}
