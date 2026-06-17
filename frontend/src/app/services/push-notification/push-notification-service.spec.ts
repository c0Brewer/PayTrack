import { PushNotificationService } from './push-notification-service';

describe('PushNotificationService', () => {
  let service: PushNotificationService;
  const originalSecureContext = Object.getOwnPropertyDescriptor(globalThis, 'isSecureContext');
  const originalNotification = Object.getOwnPropertyDescriptor(window, 'Notification');
  const originalPushManager = Object.getOwnPropertyDescriptor(window, 'PushManager');
  const originalServiceWorker = Object.getOwnPropertyDescriptor(navigator, 'serviceWorker');

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
      jsonResponse({ isConfigured: false, vapidPublicKey: null, enabled: false }),
    );

    await service.loadConfig();

    expect(service.availability()).toBe('server-not-configured');
    expect(service.enabled()).toBe(false);
  });

  it('loads enabled push config when browser and server support push', async () => {
    setServiceWorkerRegistration(createRegistration());
    vi.spyOn(window, 'fetch').mockResolvedValue(
      jsonResponse({ isConfigured: true, vapidPublicKey: 'AQID', enabled: true }),
    );

    await service.loadConfig();

    expect(service.availability()).toBe('available');
    expect(service.enabled()).toBe(true);
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
        jsonResponse({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false }),
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
          jsonResponse({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false }),
        );
      }

      return Promise.resolve(new Response(null, { status: 200 }));
    });

    await service.enable();

    expect(registration.pushManager.subscribe).toHaveBeenCalled();
    expect(service.enabled()).toBe(true);

    const saveRequest = fetchSpy.mock.calls.at(-1)!;
    expect(saveRequest[0].toString()).toContain('/api/v1/notification/push/subscribe');
    expect(JSON.parse((saveRequest[1] as RequestInit).body as string)).toEqual({
      endpoint: 'https://push.example.test/send/1',
      p256dh: 'p256dh-key',
      auth: 'auth-secret',
    });
  });

  it('enables push using an existing browser subscription', async () => {
    const existingSubscription = createSubscription('https://push.example.test/send/existing');
    const registration = createRegistration(existingSubscription);
    setServiceWorkerRegistration(registration);
    vi.spyOn(window, 'fetch').mockImplementation((input) => {
      if (input.toString().includes('/api/v1/notification/push/config')) {
        return Promise.resolve(
          jsonResponse({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false }),
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
      jsonResponse({ isConfigured: true, vapidPublicKey: 'AQID', enabled: false }),
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
    const fetchSpy = vi.spyOn(window, 'fetch').mockResolvedValue(new Response(null, { status: 200 }));

    await service.disable();

    const unsubscribeRequest = fetchSpy.mock.calls.at(-1)!;
    expect(unsubscribeRequest[0].toString()).toContain(
      '/api/v1/notification/push/unsubscribe',
    );
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

function setNotificationPermission(permission: NotificationPermission): void {
  Object.defineProperty(window, 'Notification', {
    configurable: true,
    value: { permission },
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
