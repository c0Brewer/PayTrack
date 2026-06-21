import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';

import { EuroPipe } from '../../../pipes/euro.pipe';
import {
  HomeDashboardDto,
  HomeDashboardRecentItem,
  HomeDashboardService,
} from '../../../services/home/home-dashboard-service';
import { NotificationService } from '../../../services/notification/notification-service';
import {
  Role,
  TransactionStatus,
  TransactionStatusCssClass,
  TransactionStatusLabels,
} from '../../../types/exporter';
import { BoxComponent } from '../../general/boxes/box-component/box-component';
import { StatBoxComponent } from '../../general/boxes/stat-box-component/stat-box-component';

@Component({
  selector: 'app-home-component',
  imports: [CommonModule, BoxComponent, StatBoxComponent, EuroPipe, DatePipe, RouterLink],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss',
})
export class HomeComponent implements OnInit {
  constructor(
    private readonly homeDashboardService: HomeDashboardService,
    private readonly notificationService: NotificationService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  private greetings = ['Welcome back!', 'Nice to see you again!', 'Great to have you back!'];

  greeting = this.getRandomGreeting();
  dashboard: HomeDashboardDto | null = null;
  isLoading = true;

  protected readonly role = Role;

  private getRandomGreeting(): string {
    return this.greetings[Math.floor(Math.random() * this.greetings.length)];
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  get welcomeMessage(): string {
    const userName = this.dashboard?.user.name;
    return userName ? `${this.greeting} ${userName}` : this.greeting;
  }

  loadDashboard(): void {
    this.isLoading = true;

    this.homeDashboardService.getHomeDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard = dashboard;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: unknown) => {
        this.isLoading = false;
        this.notificationService.showError(
          err instanceof Error ? err.message : 'Error while loading dashboard',
        );
        this.cdr.detectChanges();
      },
    });
  }

  getStatusLabel(status: TransactionStatus): string {
    return TransactionStatusLabels[status];
  }

  getStatusClass(status: TransactionStatus): string {
    return TransactionStatusCssClass[status];
  }

  getInvoiceReference(item: HomeDashboardRecentItem): string {
    return item.reference || item.purposeOfPayment || 'Invoice';
  }

  getPaymentRequestReference(item: HomeDashboardRecentItem): string {
    return item.purposeOfPayment || item.reference || 'Payment Request';
  }
}
