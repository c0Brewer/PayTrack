import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';

import { AuthService } from '../../../services/auth/auth-service';

@Component({
  selector: 'app-navbar-component',
  imports: [AsyncPipe],
  templateUrl: './navbar-component.html',
  styleUrl: './navbar-component.scss',
})
export class NavbarComponent {
  loggedIn$;
  currentUser$;

  constructor(private readonly authService: AuthService) {
    this.loggedIn$ = this.authService.loggedIn$;
    this.currentUser$ = this.authService.currentUser$;
  }

  logout(): void {
    this.authService.logout();
  }
}
