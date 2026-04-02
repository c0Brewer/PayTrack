import { ChangeDetectorRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { vi } from 'vitest';

import {
  NotificationService,
  NotificationMessage,
} from '../../../services/notification/notification-service';

import { NotificationComponent } from './notification-component';

describe('NotificationComponent', () => {
  let component: NotificationComponent;
  let fixture: ComponentFixture<NotificationComponent>;
  let notificationServiceMock: {
    notify$: Subject<NotificationMessage>;
    show: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    notificationServiceMock = {
      notify$: new Subject<NotificationMessage>(),
      show: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [NotificationComponent],
      providers: [
        { provide: NotificationService, useValue: notificationServiceMock },
        ChangeDetectorRef,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationComponent);
    component = fixture.componentInstance;

    // Enable fake timers
    vi.useFakeTimers();
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should add a notification and start timers', () => {
    const msg: NotificationMessage = { id: 1, message: 'Test', duration: 1000, type: 'success' };

    component['add'](msg);

    expect(component.notifications.length).toBe(1);
    expect(component.notifications[0].exiting).toBe(false);
    expect(component['timers'].has(msg.id)).toBe(true);
  });

  it('should mark notification as exiting after exit timer', () => {
    const msg: NotificationMessage = {
      id: 2,
      message: 'Exit Test',
      duration: 1000,
      type: 'success',
    };

    component['add'](msg);

    vi.advanceTimersByTime(650); // duration 1000 - EXIT_DURATION 350 = 650
    expect(component.notifications.find((n) => n.id === 2)?.exiting).toBe(true);
  });

  it('should remove notification after duration', () => {
    const msg: NotificationMessage = {
      id: 3,
      message: 'Remove Test',
      duration: 1000,
      type: 'success',
    };

    component['add'](msg);

    vi.advanceTimersByTime(1000);
    expect(component.notifications.find((n) => n.id === 3)).toBeUndefined();
    expect(component['timers'].has(3)).toBe(false);
  });
});
