import { AsyncPipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

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

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {
    this.loggedIn$ = this.authService.loggedIn$;
    this.currentUser$ = this.authService.currentUser$;
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

  settings(): void {
    this.router.navigate(['bankaccount']);
  }
}
