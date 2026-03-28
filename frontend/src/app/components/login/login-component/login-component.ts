import { isPlatformBrowser } from '@angular/common';
import { Component, inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '../../../services/auth/auth-service';

declare global {
  interface Window {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    google: any;
  }
}

@Component({
  selector: 'app-login-component',
  imports: [],
  templateUrl: './login-component.html',
  styleUrl: './login-component.scss',
})
export class LoginComponent {
  private platformId = inject(PLATFORM_ID);

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return; // SSR: skip entirely

    window.google.accounts.id.initialize({
      client_id: '165684545515-r3f0a7ph6rg438r1k208tdnf2d95ie5l.apps.googleusercontent.com', // TODO: Load from .env file
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      callback: (response: any) => this.handleCredentialResponse(response),
    });

    window.google.accounts.id.renderButton(document.getElementById('googleButton'), {
      theme: 'outline',
      size: 'large',
      type: 'standard',
      shape: 'rectangular',
      text: 'sign_in_with',
    });
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  handleCredentialResponse(response: any): void {
    const idToken = response.credential;

    this.authService.handleGoogleCallback(idToken).subscribe({
      next: (data) => {
        this.authService.storeToken(data.jwtToken);
        this.router.navigate(['']);
      },
      error: (err) => console.error(err),
    });
  }
}
