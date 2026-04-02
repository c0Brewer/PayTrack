import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { NotificationService, NotificationMessage } from './notification-service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
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
});
