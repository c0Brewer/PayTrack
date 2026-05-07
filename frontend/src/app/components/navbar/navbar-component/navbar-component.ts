import { AsyncPipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { AuthService } from '../../../services/auth/auth-service';
import { Role } from '../../../types/exporter';

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
  public hasNoBankAccounts = false;

  constructor(private readonly authService: AuthService) {
    this.loggedIn$ = this.authService.loggedIn$;
    this.currentUser$ = this.authService.currentUser$;

    this.currentUser$.pipe(takeUntilDestroyed()).subscribe((user) => {
      this.hasNoBankAccounts = !!user && (user.bankAccounts?.length ?? 0) === 0;
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
