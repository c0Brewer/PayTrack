import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

import {
  OFFLINE_READ_MESSAGE,
  OFFLINE_WRITE_MESSAGE,
  isBrowserOnline,
} from '../offline/offline-utils';

export type NotificationType = 'error' | 'success';

export interface NotificationMessage {
  id: number;
  message: string;
  duration: number; // in milliseconds
  type: NotificationType;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly _notify$ = new Subject<NotificationMessage>();
  readonly notify$ = this._notify$.asObservable();

  private idCounter = 0;

  /**
   * Show an error notification.
   * @param message  The error text to display.
   * @param duration How long to show it in ms (default: 4000).
   */
  showError(message: string, duration = 4000): void {
    if (!isBrowserOnline() && this.isOfflineMessage(message)) {
      return;
    }

    this.emit(message, 'error', duration);
  }

  /**
   * Show a success notification.
   * @param message  The success text to display.
   * @param duration How long to show it in ms (default: 3000).
   */
  showSuccess(message: string, duration = 3000): void {
    this.emit(message, 'success', duration);
  }

  private emit(message: string, type: NotificationType, duration: number): void {
    this._notify$.next({ id: ++this.idCounter, message, duration, type });
  }

  private isOfflineMessage(message: string): boolean {
    return message === OFFLINE_READ_MESSAGE || message === OFFLINE_WRITE_MESSAGE;
  }
}
