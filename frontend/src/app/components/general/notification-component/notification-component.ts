import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
  NotificationMessage,
  NotificationService,
} from '../../../services/notification/notification-service';

interface ActiveNotification extends NotificationMessage {
  exiting: boolean;
}

@Component({
  selector: 'app-notification-component',
  imports: [],
  templateUrl: './notification-component.html',
  styleUrl: './notification-component.scss',
})
export class NotificationComponent implements OnInit, OnDestroy {
  notifications: ActiveNotification[] = [];

  private sub!: Subscription;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>[]>();

  constructor(
    private readonly notificationService: NotificationService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.sub = this.notificationService.notify$.subscribe((msg) => this.add(msg));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.timers.forEach((ts) => ts.forEach(clearTimeout));
  }

  private add(msg: NotificationMessage): void {
    const notif: ActiveNotification = { ...msg, exiting: false };
    this.notifications.push(notif);

    const EXIT_DURATION = 350;
    const exitTimer = setTimeout(() => this.startExit(msg.id), msg.duration - EXIT_DURATION);
    const removeTimer = setTimeout(() => this.remove(msg.id), msg.duration);

    this.timers.set(msg.id, [exitTimer, removeTimer]);
    this.cdr.markForCheck();
  }

  private startExit(id: number): void {
    const notif = this.notifications.find((n) => n.id === id);
    if (notif) {
      notif.exiting = true;
      this.cdr.markForCheck();
    }
  }

  private remove(id: number): void {
    this.notifications = this.notifications.filter((n) => n.id !== id);
    this.timers.delete(id);
    this.cdr.markForCheck();
  }

  dismiss(id: number): void {
    const timers = this.timers.get(id);
    if (timers) timers.forEach(clearTimeout);
    this.startExit(id);
    setTimeout(() => this.remove(id), 350);
  }
}
