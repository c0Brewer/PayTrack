import { AfterViewInit, Component } from '@angular/core';
import { Router } from '@angular/router';

import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../services/auth/auth-service';
import { NotificationService } from '../../../services/notification/notification-service';

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
export class LoginComponent implements AfterViewInit {
  private codeClient: any;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly notificationService: NotificationService,
  ) {}

  async ngAfterViewInit(): Promise<void> {
    await this.authService.loadGoogleScript();

    this.codeClient = globalThis.window.google.accounts.oauth2.initCodeClient({
      client_id: environment.googleClientId,
      scope: 'openid email profile',
      ux_mode: 'popup',
      callback: (response: any) => this.handleGoogleCodeResponse(response),
    });
  }

  signInWithGoogle(): void {
    if (!this.codeClient) {
      this.notificationService.showError('Google login is not ready yet.');
      return;
    }

    this.codeClient.requestCode();
  }

  handleGoogleCodeResponse(response: any): void {
    console.log('Google response:', response);

    if (!response?.code) {
      this.notificationService.showError('Google login failed.');
      return;
    }

    this.authService.handleGoogleCallback(response.code).subscribe({
      next: (data) => {
        this.authService.storeToken(data.jwtToken);
        this.router.navigate(['']);
      },
      error: (err) => {
        console.error(err);
        this.notificationService.showError(err);
      },
    });
  }
}
