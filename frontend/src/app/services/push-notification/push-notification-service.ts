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
  devices: PushSubscriptionDevice[];
}

export interface PushSubscriptionDevice {
  id: number;
  browserName: string;
  deviceName: string;
  platform: string;
  isCurrentDevice: boolean;
  updatedAt: string;
}

interface UserAgentDataLike {
  brands?: Array<{ brand: string; version: string }>;
  mobile?: boolean;
  platform?: string;
}

interface NavigatorWithUserAgentData extends Navigator {
  userAgentData?: UserAgentDataLike;
}

@Injectable({
  providedIn: 'root',
})
export class PushNotificationService {
  readonly availability = signal<PushNotificationAvailability>('checking');
  readonly enabled = signal(false);
  readonly loading = signal(false);
  readonly devices = signal<PushSubscriptionDevice[]>([]);

  private vapidPublicKey: string | null = null;

  async loadConfig(): Promise<void> {
    this.loading.set(true);

    try {
      const localAvailability = await this.getLocalAvailability();
      if (localAvailability !== 'available') {
        this.availability.set(localAvailability);
        this.enabled.set(false);
        this.devices.set([]);
        return;
      }

      const currentEndpoint = await this.getCurrentSubscriptionEndpoint();
      const configPath = currentEndpoint
        ? `/api/v1/notification/push/config?${new URLSearchParams({ endpoint: currentEndpoint }).toString()}`
        : '/api/v1/notification/push/config';
      const config = await this.request<PushNotificationConfig>(configPath);
      if (!config.isConfigured || !config.vapidPublicKey) {
        this.availability.set('server-not-configured');
        this.enabled.set(false);
        this.devices.set(config.devices ?? []);
        return;
      }

      if (Notification.permission === 'denied') {
        this.availability.set('permission-denied');
        this.enabled.set(false);
        this.devices.set(config.devices ?? []);
        return;
      }

      this.vapidPublicKey = config.vapidPublicKey;
      this.enabled.set(config.enabled);
      this.devices.set(config.devices ?? []);
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
        (await this.createSubscription(registration, this.vapidPublicKey));
      const json = subscription.toJSON();

      await this.request<void>('/api/v1/notification/push/subscribe', {
        method: 'POST',
        body: JSON.stringify({
          endpoint: json.endpoint,
          p256dh: json.keys?.['p256dh'],
          auth: json.keys?.['auth'],
          ...this.getDeviceMetadata(),
        }),
      });

      this.enabled.set(true);
      await this.loadConfig();
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
      await this.loadConfig();
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
    return await this.withTimeout(
      navigator.serviceWorker.ready,
      5000,
      'Service worker registration timed out.',
    );
  }

  private async createSubscription(
    registration: ServiceWorkerRegistration,
    vapidPublicKey: string,
  ): Promise<PushSubscription> {
    await this.ensureNotificationPermission();

    return await this.withTimeout(
      registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: this.urlBase64ToUint8Array(vapidPublicKey),
      }),
      15000,
      'Push subscription timed out.',
    );
  }

  private async ensureNotificationPermission(): Promise<void> {
    if (Notification.permission === 'granted') {
      return;
    }

    if (Notification.permission === 'denied') {
      throw new Error('Notifications are blocked.');
    }

    if (typeof Notification.requestPermission !== 'function') {
      return;
    }

    const permission = await this.withTimeout(
      Notification.requestPermission(),
      15000,
      'Notification permission request timed out.',
    );

    if (permission !== 'granted') {
      throw new Error('Notifications were not allowed.');
    }
  }

  private async withTimeout<T>(promise: Promise<T>, timeoutMs: number, message: string): Promise<T> {
    let timeoutId: number | undefined;

    try {
      return await Promise.race([
        promise,
        new Promise<never>((_, reject) => {
          timeoutId = window.setTimeout(() => reject(new Error(message)), timeoutMs);
        }),
      ]);
    } finally {
      if (timeoutId !== undefined) {
        window.clearTimeout(timeoutId);
      }
    }
  }

  private async getCurrentSubscriptionEndpoint(): Promise<string | null> {
    const registration = await navigator.serviceWorker.getRegistration();
    const subscription = await registration?.pushManager.getSubscription();
    return subscription?.endpoint ?? null;
  }

  private getDeviceMetadata(): {
    browserName: string;
    deviceName: string;
    platform: string;
  } {
    const userAgent = navigator.userAgent;
    const userAgentData = (navigator as NavigatorWithUserAgentData).userAgentData;
    const platform = userAgentData?.platform || navigator.platform || 'Unknown platform';

    return {
      browserName: this.detectBrowserName(userAgent, userAgentData),
      deviceName: this.detectDeviceName(userAgent, platform, userAgentData?.mobile ?? false),
      platform: this.truncate(platform, 120),
    };
  }

  private detectBrowserName(userAgent: string, userAgentData?: UserAgentDataLike): string {
    const brands = userAgentData?.brands ?? [];
    if (brands.some((item) => /edge/i.test(item.brand))) return 'Microsoft Edge';
    if (brands.some((item) => /chrome/i.test(item.brand))) return 'Chrome';

    if (/SamsungBrowser/i.test(userAgent)) return 'Samsung Internet';
    if (/Edg\//i.test(userAgent)) return 'Microsoft Edge';
    if (/Firefox\//i.test(userAgent)) return 'Firefox';
    if (/Chrome\//i.test(userAgent) || /CriOS\//i.test(userAgent)) return 'Chrome';
    if (/Safari\//i.test(userAgent)) return 'Safari';

    const brand = brands.find((item) => !/not/i.test(item.brand));
    return this.truncate(brand?.brand ?? 'Unknown browser', 120);
  }

  private detectDeviceName(
    userAgent: string,
    platform: string,
    isMobileUserAgentData: boolean,
  ): string {
    const samsungModel = userAgent.match(/\bSM-[A-Z0-9]+\b/i)?.[0];
    if (samsungModel) return this.truncate(`Samsung ${samsungModel.toUpperCase()}`, 160);

    if (/Android/i.test(userAgent)) {
      return isMobileUserAgentData ? 'Android phone' : 'Android device';
    }

    if (/iPhone/i.test(userAgent)) return 'iPhone';
    if (/iPad/i.test(userAgent)) return 'iPad';
    if (/Windows/i.test(userAgent)) return 'Windows device';
    if (/Macintosh/i.test(userAgent)) return 'Mac';

    return this.truncate(platform || 'Unknown device', 160);
  }

  private truncate(value: string, maxLength: number): string {
    return value.length > maxLength ? value.slice(0, maxLength) : value;
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
      const message = await this.getErrorMessage(response);
      throw new Error(message);
    }

    if (response.status === 204) {
      return undefined as T;
    }

    const text = await response.text();
    return text ? (JSON.parse(text) as T) : (undefined as T);
  }

  private async getErrorMessage(response: Response): Promise<string> {
    const fallback = `Request failed with status ${response.status}`;
    const text = await response.text();

    if (!text) {
      return fallback;
    }

    try {
      const parsed = JSON.parse(text) as unknown;

      if (this.hasDetail(parsed)) {
        return parsed.detail;
      }

      if (Array.isArray(parsed)) {
        const validationMessage = parsed
          .map((item) => (this.hasErrorMessage(item) ? item.errorMessage : null))
          .find((message): message is string => !!message);

        return validationMessage ?? fallback;
      }
    } catch {
      return text;
    }

    return fallback;
  }

  private hasDetail(value: unknown): value is { detail: string } {
    return (
      typeof value === 'object' &&
      value !== null &&
      'detail' in value &&
      typeof value.detail === 'string'
    );
  }

  private hasErrorMessage(value: unknown): value is { errorMessage: string } {
    return (
      typeof value === 'object' &&
      value !== null &&
      'errorMessage' in value &&
      typeof value.errorMessage === 'string'
    );
  }
}
