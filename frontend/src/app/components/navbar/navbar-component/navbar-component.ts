import { AsyncPipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter, startWith, switchMap, take } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';
import { PaymentRequestByTeamService } from '../../../services/payment-request-by-team/payment-request-by-team-service';
import { PaymentRequestByUserService } from '../../../services/payment-request-by-user/payment-request-by-user-service';
import { Role, TransactionStatus } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-navbar-component',
  imports: [AsyncPipe, ModalComponent, RouterLink, RouterLinkActive],
  templateUrl: './navbar-component.html',
  styleUrl: './navbar-component.scss',
})
export class NavbarComponent {
  loggedIn$;
  currentUser$;
  protected readonly role = Role;
  protected readonly mobileMenuOpen = signal(false);
  protected readonly managementMenuOpen = signal(false);
  protected readonly requestsMenuOpen = signal(false);
  protected readonly signOutModalOpen = signal(false);
  protected readonly currentUrl = signal('');
  protected readonly submittedCount = signal(0);
  protected readonly teamRequestCount = signal(0);
  public hasNoBankAccounts = false;

  constructor(
    private readonly authService: AuthService,
    private readonly paymentRequestService: PaymentRequestByUserService,
    private readonly teamRequestService: PaymentRequestByTeamService,
    private readonly router: Router,
  ) {
    this.loggedIn$ = this.authService.loggedIn$;
    this.currentUser$ = this.authService.currentUser$;
    this.currentUrl.set(this.router.url);

    this.currentUser$.pipe(takeUntilDestroyed()).subscribe((user) => {
      this.hasNoBankAccounts = !!user && (user.bankAccounts?.length ?? 0) === 0;
    });

    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        startWith(null),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        this.currentUrl.set(this.router.url);
      });
  }

  ngOnInit(): void {
    this.currentUser$
      .pipe(
        filter((user) => user?.role === Role.ADMIN),
        take(1),
        switchMap(() =>
          this.paymentRequestService.getPaymentRequestsByUser({ Status: 0, Limit: 1 }),
        ),
      )
      .subscribe({
        next: (result) => this.submittedCount.set(result?.totalCount ?? 0),
        error: () => {},
      });

    this.currentUser$
      .pipe(
        filter((user) => !!user),
        take(1),
        switchMap((user) =>
          this.teamRequestService.getPaymentRequestsByTeam({
            Status: TransactionStatus.Submitted,
            UserId: user!.id,
            Limit: 1,
          }),
        ),
      )
      .subscribe({
        next: (result) => this.teamRequestCount.set(result?.totalCount ?? 0),
        error: () => {},
      });
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((isOpen) => !isOpen);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
    this.managementMenuOpen.set(false);
    this.requestsMenuOpen.set(false);
  }

  toggleManagementMenu(): void {
    this.managementMenuOpen.update((isOpen) => !isOpen);
  }

  toggleRequestsMenu(): void {
    this.requestsMenuOpen.update((isOpen) => !isOpen);
  }

  openSignOutModal(): void {
    this.signOutModalOpen.set(true);
  }

  closeSignOutModal(): void {
    this.signOutModalOpen.set(false);
  }

  isManagementMenuExpanded(): boolean {
    return this.managementMenuOpen() || this.isManagementRouteActive();
  }

  isRequestsMenuExpanded(): boolean {
    return this.requestsMenuOpen() || this.isRequestsRouteActive();
  }

  private isManagementRouteActive(): boolean {
    const url = this.currentUrl();
    return (
      url.startsWith('/user') ||
      url.startsWith('/team') ||
      url.startsWith('/cost-centre') ||
      url.startsWith('/season')
    );
  }

  private isRequestsRouteActive(): boolean {
    const url = this.currentUrl();
    return (
      url.startsWith('/create-payment-request') ||
      url.startsWith('/payment-requests-by-team') ||
      url.startsWith('/requests')
    );
  }

  logout(): void {
    this.closeSignOutModal();
    this.closeMobileMenu();
    this.authService.logout();
  }
}
