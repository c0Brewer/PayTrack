import { AsyncPipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { PaymentRequestStatusRefreshService } from '../../../services/payment-request-by-user/payment-request-status-refresh-service';
import { Role, TransactionStatus, UserDto } from '../../../types/exporter';

@Component({
  selector: 'app-navbar-component',
  imports: [AsyncPipe, RouterLink, RouterLinkActive],
  templateUrl: './navbar-component.html',
  styleUrl: './navbar-component.scss',
})
export class NavbarComponent {
  loggedIn$;
  currentUser$;
  protected readonly role = Role;
  protected readonly mobileMenuOpen = signal(false);
  protected readonly submittedCount = signal(0);
  protected readonly teamRequestCount = signal(0);
  protected readonly invoiceChangesRequestedCount = signal(0);
  public hasNoBankAccounts = false;
  private currentUser: UserDto | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly teamRequestService: PaymentRequestByTeamService,
    private readonly statusRefreshService: PaymentRequestStatusRefreshService,
    private readonly router: Router,
  ) {
    this.loggedIn$ = this.authService.loggedIn$;
    this.currentUser$ = this.authService.currentUser$;

    this.currentUser$.pipe(takeUntilDestroyed()).subscribe((user) => {
      this.currentUser = user;
      this.hasNoBankAccounts = !!user && (user.bankAccounts?.length ?? 0) === 0;
      this.refreshCounts();
    });

    this.statusRefreshService.refreshRequested$.pipe(takeUntilDestroyed()).subscribe(() => {
      this.refreshCounts();
    });

    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        this.refreshCounts();
      });
  }

  ngOnInit(): void {
    this.refreshCounts();
  }

  private refreshCounts(): void {
    if (!this.currentUser) {
      return;
    }

    this.loadSubmittedCount(this.currentUser);
    this.loadTeamRequestCount(this.currentUser);
    this.loadInvoiceChangesRequestedCount(this.currentUser);
  }

  private loadSubmittedCount(user: UserDto): void {
    if (user.role !== Role.ADMIN) {
      this.submittedCount.set(0);
      return;
    }

    this.paymentRequestService.getPaymentRequestsByUser({ Status: 0, Limit: 1 }).subscribe({
      next: (result) => this.submittedCount.set(result?.totalCount ?? 0),
      error: () => {},
    });
  }

  private loadTeamRequestCount(user: UserDto): void {
    this.teamRequestService
      .getPaymentRequestsByTeam({
        Status: TransactionStatus.Submitted,
        UserId: user.id,
        Limit: 1,
      })
      .subscribe({
        next: (result) => this.teamRequestCount.set(result?.totalCount ?? 0),
        error: () => {},
      });
  }

  private loadInvoiceChangesRequestedCount(user: UserDto): void {
    this.paymentRequestService
      .getPaymentRequestsByUser({
        Status: TransactionStatus.ChangesRequested,
        UserId: user.id,
        Limit: 1,
      })
      .subscribe({
        next: (result) => this.invoiceChangesRequestedCount.set(result?.totalCount ?? 0),
        error: () => {},
      });
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((isOpen) => !isOpen);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  logout(): void {
    this.closeMobileMenu();
    this.authService.logout();
  }
}
