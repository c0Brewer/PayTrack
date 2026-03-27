import { Component, inject, PLATFORM_ID } from '@angular/core';
import { AuthService } from '../../../services/auth/auth-service';
import { isPlatformBrowser } from '@angular/common';

declare global {
  interface Window {
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

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return; // SSR: skip entirely

    window.google.accounts.id.initialize({
      client_id: "165684545515-r3f0a7ph6rg438r1k208tdnf2d95ie5l.apps.googleusercontent.com", // TODO: Load from .env file
      callback: (response: any) => this.handleCredentialResponse(response)
    });

    window.google.accounts.id.renderButton(
      document.getElementById("googleButton"),
      {
        theme: "outline",
        size: "large",
        type: "standard",
        shape: "rectangular"
      }
    );
  }

  handleCredentialResponse(response: any) {
    const idToken = response.credential;

    console.log("Google ID Token:", idToken);

    this.authService.handleGoogleCallback(idToken).subscribe({
      next: (data) => {
        console.log(data)
        this.authService.storeToken(data.jwtToken);
      },
      error: (err) => console.error(err)
    });
  }
}
