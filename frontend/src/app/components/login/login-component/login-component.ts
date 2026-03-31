import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { environment } from '../../../../environments/environment';
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
export class LoginComponent implements AfterViewInit {
  @ViewChild('googleButton', { static: false }) googleButton!: ElementRef;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  ngAfterViewInit(): void {
    this.authService.loadGoogleScript();

    globalThis.window.google.accounts.id.initialize({
      client_id: environment.googleClientId,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      callback: (response: any) => this.handleCredentialResponse(response),
    });

    globalThis.window.google.accounts.id.renderButton(this.googleButton.nativeElement, {
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
