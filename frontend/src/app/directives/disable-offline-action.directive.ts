import { Directive, HostBinding, HostListener, Input, effect, inject } from '@angular/core';

import { NotificationService } from '../services/notification/notification-service';
import { OfflineService } from '../services/offline/offline-service';

@Directive({
  selector: 'button[appDisableOfflineAction]',
  standalone: true,
})
export class DisableOfflineActionDirective {
  @Input() appDisableOfflineAction = '';

  @HostBinding('attr.title') title: string | null = null;
  @HostBinding('attr.aria-disabled') ariaDisabled: 'true' | null = null;
  @HostBinding('class.offline-action-disabled') isOfflineActionDisabled = false;

  private readonly offlineService = inject(OfflineService);
  private readonly notificationService = inject(NotificationService);

  constructor() {
    effect(() => {
      const offline = this.offlineService.isOffline();
      this.isOfflineActionDisabled = offline;
      this.ariaDisabled = offline ? 'true' : null;
      this.title = offline
        ? this.appDisableOfflineAction || this.offlineService.offlineWriteMessage
        : null;
    });
  }

  @HostListener('click', ['$event'])
  onClick(event: Event): void {
    if (!this.offlineService.isOffline()) {
      return;
    }

    event.preventDefault();
    event.stopImmediatePropagation();
    event.stopPropagation();
    this.notificationService.showError(
      this.appDisableOfflineAction || this.offlineService.offlineWriteMessage,
    );
  }
}
