import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { OfflineStatusComponent } from './components/general/offline-status-component/offline-status-component';
import { NavbarComponent } from './components/navbar/navbar-component/navbar-component';
import { NotificationComponent } from './components/general/notification-component/notification-component';
import { OfflineInvoiceSubmissionQueueService } from './services/offline/offline-invoice-submission-queue.service';
import { OfflineService } from './services/offline/offline-service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, NotificationComponent, OfflineStatusComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('PayTrack');

  constructor(
    private readonly router: Router,
    private readonly offlineService: OfflineService,
    private readonly offlineInvoiceSubmissionQueueService: OfflineInvoiceSubmissionQueueService,
  ) {
    this.offlineService.init();
    this.offlineInvoiceSubmissionQueueService.init();
  }

  protected showNavbar(): boolean {
    return !this.router.url.startsWith('/login') && !this.router.url.startsWith('/initial-setup');
  }
}
