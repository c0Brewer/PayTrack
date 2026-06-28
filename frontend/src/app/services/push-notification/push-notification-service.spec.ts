//AI helped with the test cases

import { PushNotificationService } from './push-notification-service';

describe('PushNotificationService', () => {
  let service: PushNotificationService;
  const originalSecureContext = Object.getOwnPropertyDescriptor(globalThis, 'isSecureContext');
  const originalNotification = Object.getOwnPropertyDescriptor(window, 'Notification');
  const originalPushManager = Object.getOwnPropertyDescriptor(window, 'PushManager');
  const originalServiceWorker = Object.getOwnPropertyDescriptor(navigator, 'serviceWorker');
  const originalUserAgent = Object.getOwnPropertyDescriptor(navigator, 'userAgent');
  const originalPlatform = Object.getOwnPropertyDescriptor(navigator, 'platform');
  const originalUserAgentData = Object.getOwnPropertyDescriptor(navigator, 'userAgentData');

  beforeEach(() => {
    localStorage.clear();
    service = new PushNotificationService();
    setSecureContext(true);
    setNotificationPermission('default');
    setPushManagerSupport(true);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    restoreProperty(globalThis, 'isSecureContext', originalSecureContext);
    restoreProperty(window, 'Notification', originalNotification);
    restoreProperty(window, 'PushManager', originalPushManager);
    restoreProperty(navigator, 'serviceWorker', originalServiceWorker);
    restoreProperty(navigator, 'userAgent', originalUserAgent);
    restoreProperty(navigator, 'platform', originalPlatform);
    restoreProperty(navigator, 'userAgentData', originalUserAgentData);
  });

  it('marks push unavailable outside a secure context', async () => {
    setSecureContext(false);
    setServiceWorkerRegistration(createRegistration());

    await service.loadConfig();

    expect(service.availability()).toBe('unsupported-context');
    expect(service.enabled()).toBe(false);
  });

  it('marks push unavailable when browser APIs are missing', async () => {
    restoreProperty(window, 'PushManager', undefined);
    setServiceWorkerRegistration(createRegistration());

    await service.loadConfig();

    expect(service.availability()).toBe('unsupported-browser');
  });

  it('marks push unavailable when notification permission is denied', async () => {
    setNotificationPermission('denied');
    setServiceWorkerRegistration(createRegistration());

    await service.loadConfig();

    expect(service.availability()).toBe('permission-denied');
  });

  it('marks push unavailable when the service worker cannot be registered', async () => {
    setServiceWorkerRegistration(null);
    vi.spyOn(window, 'fetch').mockResolvedValue(
      new Response('<html></html>', {
        status: 200,
        headers: { 'content-type': 'text/html' },
      }),
    );

    await service.loadConfig();

    expect(service.availability()).toBe('service-worker-disabled');
  });

  it('marks push unavailable when the server has no VAPID config', async () => {
    setServiceWorkerRegistration(createRegistration());
    vi.spyOn(window, 'fetch').mockResolvedValue(
      jsonResponse(pushConfig({ isConfigured: false, vapidPublicKey: null, enabled: false })),
    );

    await service.loadConfig();

    expect(service.availability()).toBe('server-not-configured');
    expect(service.enabled()).toBe(false);
  });

  it('loads enabled push config when browser and server support push', async () => {
    setServiceWorkerRegistration(createRegistration());
    vi.spyOn(window, 'fetch').mockResolvedValue(
      jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true })),
    );

    await service.loadConfig();

    expect(service.availability()).toBe('available');
    expect(service.enabled()).toBe(true);
  });

  it('loads current-device config with endpoint and stores active devices', async () => {
    const existingSubscription = createSubscription('https://push.example.test/send/current');
    setServiceWorkerRegistration(createRegistration(existingSubscription));
    const devices = [
      {
        id: 1,
        browserName: 'Chrome',
        deviceName: 'Windows device',
        platform: 'Windows',
        isCurrentDevice: true,
        updatedAt: '2026-06-25T12:00:00Z',
      },
    ];
    const fetchSpy = vi
      .spyOn(window, 'fetch')
      .mockResolvedValue(
        jsonResponse(
          pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true, devices }),
        ),
      );

    await service.loadConfig();

    expect(fetchSpy.mock.calls[0][0].toString()).toContain(
      'endpoint=https%3A%2F%2Fpush.example.test%2Fsend%2Fcurrent',
    );
    expect(service.devices()).toEqual(devices);
  });

  it('marks unavailable when permission changes to denied while loading server config', async () => {
    setServiceWorkerRegistration(createRegistration());
    vi.spyOn(window, 'fetch').mockImplementation(() => {
      setNotificationPermission('denied');
      return Promise.resolve(
        jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true })),
      );
    });

    await service.loadConfig();

    expect(service.availability()).toBe('permission-denied');
    expect(service.enabled()).toBe(false);
  });

  it('registers the Angular service worker when no registration exists yet', async () => {
    const registration = createRegistration();
    const serviceWorker = setServiceWorkerRegistration(null, registration);
    const fetchSpy = vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input === '/ngsw-worker.js') {
        return Promise.resolve(
          new Response('worker', {
            status: 200,
            headers: { 'content-type': 'text/javascript' },
          }),
        );
      }

      return Promise.resolve(
        jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false })),
      );
    });

    await service.loadConfig();

    expect(serviceWorker.register).toHaveBeenCalledWith('/ngsw-worker.js');
    expect(fetchSpy.mock.calls.some(([input]) => input === '/ngsw-worker.js')).toBe(true);
    expect(service.availability()).toBe('available');
  });

  it('enables push by subscribing and saving subscription keys', async () => {
    const registration = createRegistration();
    setServiceWorkerRegistration(registration);
    const fetchSpy = vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input.toString().includes('/api/v1/notification/push/config')) {
        return Promise.resolve(
          jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true })),
        );
      }

      return Promise.resolve(new Response(null, { status: 200 }));
    });

    await service.enable();

    expect(registration.pushManager.subscribe).toHaveBeenCalled();
    expect(service.enabled()).toBe(true);

    const saveRequest = fetchSpy.mock.calls.find(([input]) =>
      input.toString().includes('/api/v1/notification/push/subscribe'),
    )!;
    expect(saveRequest[0].toString()).toContain('/api/v1/notification/push/subscribe');
    expect(JSON.parse((saveRequest[1] as RequestInit).body as string)).toEqual(
      expect.objectContaining({
        endpoint: 'https://push.example.test/send/1',
        p256dh: 'p256dh-key',
        auth: 'auth-secret',
        browserName: expect.any(String),
        deviceName: expect.any(String),
        platform: expect.any(String),
      }),
    );
  });

  it('requests notification permission and sends detected Samsung browser metadata', async () => {
    setNotificationPermission('default', vi.fn().mockResolvedValue('granted'));
    setNavigatorDevice({
      userAgent:
        'Mozilla/5.0 (Linux; Android 14; SM-G991B) AppleWebKit/537.36 SamsungBrowser/26.0 Chrome/122.0 Safari/537.36',
      platform: 'Android',
      userAgentData: {
        brands: [{ brand: 'Samsung Internet', version: '26' }],
        mobile: true,
        platform: 'Android',
      },
    });
    const registration = createRegistration();
    setServiceWorkerRegistration(registration);
    const fetchSpy = vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input.toString().includes('/api/v1/notification/push/config')) {
        return Promise.resolve(
          jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true })),
        );
      }

      return Promise.resolve(new Response(null, { status: 200 }));
    });

    await service.enable();

    const saveRequest = fetchSpy.mock.calls.find(([input]) =>
      input.toString().includes('/api/v1/notification/push/subscribe'),
    )!;
    expect(window.Notification.requestPermission).toHaveBeenCalled();
    expect(JSON.parse((saveRequest[1] as RequestInit).body as string)).toEqual(
      expect.objectContaining({
        browserName: 'Samsung Internet',
        deviceName: 'Samsung SM-G991B',
        platform: 'Android',
      }),
    );
  });

  it('does not enable push when notification permission is dismissed', async () => {
    setNotificationPermission('default', vi.fn().mockResolvedValue('default'));
    setServiceWorkerRegistration(createRegistration());
    vi.spyOn(window, 'fetch').mockResolvedValue(
      jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false })),
    );

    await service.loadConfig();

    await expect(service.enable()).rejects.toThrow('Push notifications could not be enabled.');
    expect(service.enabled()).toBe(false);
  });

  it('handles backend problem detail errors while enabling push', async () => {
    setServiceWorkerRegistration(createRegistration());
    vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input.toString().includes('/api/v1/notification/push/config')) {
        return Promise.resolve(
          jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true })),
        );
      }

      return Promise.resolve(
        new Response(
          JSON.stringify({ detail: 'The push subscription endpoint is not supported.' }),
          {
            status: 400,
            headers: { 'content-type': 'application/json' },
          },
        ),
      );
    });

    await expect(service.enable()).rejects.toThrow('Push notifications could not be enabled.');
    expect(service.enabled()).toBe(false);
  });

  it('enables push using an existing browser subscription', async () => {
    const existingSubscription = createSubscription('https://push.example.test/send/existing');
    const registration = createRegistration(existingSubscription);
    setServiceWorkerRegistration(registration);
    vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input.toString().includes('/api/v1/notification/push/config')) {
        return Promise.resolve(
          jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true })),
        );
      }

      return Promise.resolve(new Response(null, { status: 200 }));
    });

    await service.enable();

    expect(registration.pushManager.subscribe).not.toHaveBeenCalled();
    expect(service.enabled()).toBe(true);
  });

  it('sets permission-denied availability when enabling fails after permission is denied', async () => {
    const registration = createRegistration(null, true);
    setServiceWorkerRegistration(registration);
    vi.spyOn(window, 'fetch').mockResolvedValue(
      jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false })),
    );

    await service.loadConfig();
    setNotificationPermission('denied');

    await expect(service.enable()).rejects.toThrow('Push notifications could not be enabled.');
    expect(service.availability()).toBe('permission-denied');
    expect(service.enabled()).toBe(false);
  });

  it('disables push by unregistering the backend and browser subscription', async () => {
    const subscription = createSubscription('https://push.example.test/send/disable');
    setServiceWorkerRegistration(createRegistration(subscription));
    const fetchSpy = vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input.toString().includes('/api/v1/notification/push/config')) {
        return Promise.resolve(
          jsonResponse(pushConfig({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false })),
        );
      }

      return Promise.resolve(new Response(null, { status: 200 }));
    });

    await service.disable();

    const unsubscribeRequest = fetchSpy.mock.calls.find(([input]) =>
      input.toString().includes('/api/v1/notification/push/unsubscribe'),
    )!;
    expect(unsubscribeRequest[0].toString()).toContain('/api/v1/notification/push/unsubscribe');
    expect(JSON.parse((unsubscribeRequest[1] as RequestInit).body as string)).toEqual({
      endpoint: 'https://push.example.test/send/disable',
    });
    expect(subscription.unsubscribe).toHaveBeenCalled();
    expect(service.enabled()).toBe(false);
  });

  it('disables local state when no service worker registration exists', async () => {
    setServiceWorkerRegistration(null);

    await service.disable();

    expect(service.enabled()).toBe(false);
  });
});

function setSecureContext(value: boolean): void {
  Object.defineProperty(globalThis, 'isSecureContext', {
    configurable: true,
    value,
  });
}

function setNotificationPermission(
  permission: NotificationPermission,
  requestPermission?: () => Promise<NotificationPermission>,
): void {
  Object.defineProperty(window, 'Notification', {
    configurable: true,
    value: { permission, requestPermission },
  });
}

function setPushManagerSupport(enabled: boolean): void {
  if (!enabled) {
    restoreProperty(window, 'PushManager', undefined);
    return;
  }

  Object.defineProperty(window, 'PushManager', {
    configurable: true,
    value: class PushManager {},
  });
}

function setServiceWorkerRegistration(
  registration: ServiceWorkerRegistration | null,
  registeredRegistration = registration,
): { register: ReturnType<typeof vi.fn> } {
  const serviceWorker = {
    getRegistration: vi.fn().mockResolvedValue(registration ?? undefined),
    register: vi.fn().mockResolvedValue(registeredRegistration),
    ready: Promise.resolve(registeredRegistration),
  };

  Object.defineProperty(navigator, 'serviceWorker', {
    configurable: true,
    value: serviceWorker,
  });

  return serviceWorker;
}

function createRegistration(
  existingSubscription: PushSubscription | null = null,
  rejectSubscribe = false,
): ServiceWorkerRegistration {
  const pushManager = {
    getSubscription: vi.fn().mockResolvedValue(existingSubscription),
    subscribe: vi
      .fn()
      .mockImplementation(() =>
        rejectSubscribe
          ? Promise.reject(new Error('Subscription failed'))
          : Promise.resolve(createSubscription('https://push.example.test/send/1')),
      ),
  };

  return {
    active: {} as ServiceWorker,
    pushManager,
  } as unknown as ServiceWorkerRegistration;
}

function createSubscription(endpoint: string): PushSubscription {
  return {
    endpoint,
    toJSON: () => ({
      endpoint,
      keys: {
        p256dh: 'p256dh-key',
        auth: 'auth-secret',
      },
    }),
    unsubscribe: vi.fn().mockResolvedValue(true),
  } as unknown as PushSubscription;
}

function jsonResponse(body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status: 200,
    headers: { 'content-type': 'application/json' },
  });
}

function pushConfig(config: {
  isConfigured: boolean;
  vapidPublicKey: string | null;
  enabled: boolean;
  devices?: unknown[];
}): {
  isConfigured: boolean;
  vapidPublicKey: string | null;
  enabled: boolean;
  devices: unknown[];
} {
  return {
    ...config,
    devices: config.devices ?? [],
  };
}

function setNavigatorDevice(config: {
  userAgent: string;
  platform: string;
  userAgentData?: {
    brands?: Array<{ brand: string; version: string }>;
    mobile?: boolean;
    platform?: string;
  };
}): void {
  Object.defineProperty(navigator, 'userAgent', {
    configurable: true,
    value: config.userAgent,
  });
  Object.defineProperty(navigator, 'platform', {
    configurable: true,
    value: config.platform,
  });
  Object.defineProperty(navigator, 'userAgentData', {
    configurable: true,
    value: config.userAgentData,
  });
}

function restoreProperty<T extends object>(
  target: T,
  property: PropertyKey,
  descriptor: PropertyDescriptor | undefined,
): void {
  if (descriptor) {
    Object.defineProperty(target, property, descriptor);
    return;
  }

  delete (target as Record<PropertyKey, unknown>)[property];
}
