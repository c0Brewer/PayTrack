import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { OFFLINE_READ_MESSAGE, OFFLINE_WRITE_MESSAGE } from '../offline/offline-utils';

import { NotificationService, NotificationMessage } from './notification-service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
    setBrowserOnline(true);
  });

  afterEach(() => {
    setBrowserOnline(true);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should emit an error notification with showError', async () => {
    const testMessage = 'Error occurred';
    const promise = firstValueFrom(service.notify$);
    service.showError(testMessage);
    const msg: NotificationMessage = await promise;
    expect(msg.message).toBe(testMessage);
    expect(msg.type).toBe('error');
    expect(msg.duration).toBe(4000);
  });

  it('should emit a success notification with showSuccess', async () => {
    const testMessage = 'Success!';
    const promise = firstValueFrom(service.notify$);
    service.showSuccess(testMessage);
    const msg: NotificationMessage = await promise;
    expect(msg.message).toBe(testMessage);
    expect(msg.type).toBe('success');
    expect(msg.duration).toBe(3000);
  });

  it('should use custom durations if provided', async () => {
    const testMessage = 'Custom duration';
    const customDuration = 5000;
    const promise = firstValueFrom(service.notify$);
    service.showError(testMessage, customDuration);
    const msg: NotificationMessage = await promise;
    expect(msg.duration).toBe(customDuration);
  });

  it('should not emit offline error notifications while offline', async () => {
    setBrowserOnline(false);
    const emitted: NotificationMessage[] = [];
    const subscription = service.notify$.subscribe((msg) => emitted.push(msg));

    service.showError(OFFLINE_READ_MESSAGE);
    service.showError(OFFLINE_WRITE_MESSAGE);

    await new Promise((resolve) => setTimeout(resolve));

    expect(emitted).toEqual([]);
    subscription.unsubscribe();
  });
});

function setBrowserOnline(online: boolean): void {
  Object.defineProperty(navigator, 'onLine', {
    configurable: true,
    get: () => online,
  });
}
