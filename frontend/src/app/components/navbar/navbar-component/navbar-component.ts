import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { Observable } from 'rxjs';

import { AuthService } from '../../../services/auth/auth-service';

@Component({
  selector: 'app-navbar-component',
  imports: [AsyncPipe],
  templateUrl: './navbar-component.html',
  styleUrl: './navbar-component.scss',
})
export class NavbarComponent {
  isLoggedIn$: Observable<boolean>;

  constructor(private authService: AuthService) {
    this.isLoggedIn$ = this.authService.isLoggedIn$();
  }

  logout(): void {
    this.authService.logout();
  }
}
