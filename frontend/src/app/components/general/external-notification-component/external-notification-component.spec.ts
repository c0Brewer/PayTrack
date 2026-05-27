import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { ExternalNotificationService } from '../../../services/external-notification/external-notification-service';
import { NotificationService } from '../../../services/notification/notification-service';

import { ExternalNotificationComponent } from './external-notification-component';

describe('ExternalNotificationComponent', () => {
  let component: ExternalNotificationComponent;
  let fixture: ComponentFixture<ExternalNotificationComponent>;

  const externalNotificationMock = {
    sendEmail: vi.fn(),
    sendSlack: vi.fn(),
  };

  const notificationMock = {
    showSuccess: vi.fn(),
    showError: vi.fn(),
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [ExternalNotificationComponent],
      providers: [
        { provide: ExternalNotificationService, useValue: externalNotificationMock },
        { provide: NotificationService, useValue: notificationMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ExternalNotificationComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.componentRef.setInput('type', 'email');
    fixture.componentRef.setInput('recipientEmail', 'user@example.com');
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('title', () => {
    it('returns email notification title when type is email', () => {
      component.type = 'email';
      expect(component.title).toBe('Send Email Notification');
    });

    it('returns slack notification title when type is slack', () => {
      component.type = 'slack';
      expect(component.title).toBe('Send Slack Notification');
    });
  });

  describe('ngOnInit', () => {
    it('copies defaultSubject and defaultMessage to editable form fields', () => {
      component.type = 'email';
      component.recipientEmail = 'user@example.com';
      component.defaultSubject = 'My Subject';
      component.defaultMessage = 'My Message';

      component.ngOnInit();

      expect(component.subject).toBe('My Subject');
      expect(component.message).toBe('My Message');
    });

    it('leaves subject and message empty when defaults are not provided', () => {
      component.type = 'slack';
      component.recipientEmail = 'user@example.com';

      component.ngOnInit();

      expect(component.subject).toBe('');
      expect(component.message).toBe('');
    });
  });

  describe('onClose', () => {
    it('emits closeEvent', () => {
      const spy = vi.spyOn(component.closeEvent, 'emit');
      component.onClose();
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('onSend', () => {
    it('calls sendEmail with recipient, subject, and message when type is email', () => {
      externalNotificationMock.sendEmail.mockReturnValue(of(undefined));
      component.type = 'email';
      component.recipientEmail = 'user@example.com';
      component.subject = 'Reminder Subject';
      component.message = 'Reminder Body';

      component.onSend();

      expect(externalNotificationMock.sendEmail).toHaveBeenCalledWith(
        'user@example.com',
        'Reminder Subject',
        'Reminder Body',
      );
    });

    it('calls sendSlack with recipient and message when type is slack', () => {
      externalNotificationMock.sendSlack.mockReturnValue(of(undefined));
      component.type = 'slack';
      component.recipientEmail = 'user@example.com';
      component.message = 'Slack reminder';

      component.onSend();

      expect(externalNotificationMock.sendSlack).toHaveBeenCalledWith(
        'user@example.com',
        'Slack reminder',
      );
    });

    it('shows success toast and emits closeEvent after a successful send', () => {
      externalNotificationMock.sendEmail.mockReturnValue(of(undefined));
      const closeSpy = vi.spyOn(component.closeEvent, 'emit');
      component.type = 'email';
      component.recipientEmail = 'user@example.com';

      component.onSend();

      expect(notificationMock.showSuccess).toHaveBeenCalledWith('Notification sent successfully.');
      expect(closeSpy).toHaveBeenCalled();
    });

    it('shows error toast and does not emit closeEvent when the send fails', () => {
      externalNotificationMock.sendEmail.mockReturnValue(
        throwError(() => new Error('Dispatch failed')),
      );
      const closeSpy = vi.spyOn(component.closeEvent, 'emit');
      component.type = 'email';
      component.recipientEmail = 'user@example.com';

      component.onSend();

      expect(notificationMock.showError).toHaveBeenCalledWith(
        'Failed to send notification: Dispatch failed',
      );
      expect(closeSpy).not.toHaveBeenCalled();
    });

    it('resets sending to false after a successful send', () => {
      externalNotificationMock.sendSlack.mockReturnValue(of(undefined));
      component.type = 'slack';
      component.recipientEmail = 'user@example.com';

      component.onSend();

      expect(component.sending).toBe(false);
    });

    it('resets sending to false after a failed send', () => {
      externalNotificationMock.sendSlack.mockReturnValue(throwError(() => new Error('Error')));
      component.type = 'slack';
      component.recipientEmail = 'user@example.com';

      component.onSend();

      expect(component.sending).toBe(false);
    });
  });
});
