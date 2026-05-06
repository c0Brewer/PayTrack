import { AfterViewInit, Component } from '@angular/core';
import { Router } from '@angular/router';

import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../services/auth/auth-service';
import { NotificationService } from '../../../services/notification/notification-service';

interface GoogleCodeResponse {
  code?: string;
}

interface GoogleCodeClient {
  requestCode(): void;
}

interface GoogleCodeClientConfig {
  client_id: string;
  scope: string;
  ux_mode: 'popup' | 'redirect';
  callback: (response: GoogleCodeResponse) => void;
}

interface GoogleIdentityServices {
  accounts: {
    oauth2: {
      initCodeClient(config: GoogleCodeClientConfig): GoogleCodeClient;
    };
  };
}

declare global {
  interface Window {
    google?: GoogleIdentityServices;
  }
}

@Component({
  selector: 'app-login-component',
  imports: [],
  templateUrl: './login-component.html',
  styleUrl: './login-component.scss',
})
export class LoginComponent implements AfterViewInit {
  private codeClient?: GoogleCodeClient;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly notificationService: NotificationService,
  ) {}

  async ngAfterViewInit(): Promise<void> {
    await this.authService.loadGoogleScript();

    const google = globalThis.window.google;
    if (!google) {
      this.notificationService.showError('Google login is not ready yet.');
      return;
    }

    this.codeClient = google.accounts.oauth2.initCodeClient({
      client_id: environment.googleClientId,
      scope: 'openid email profile',
      ux_mode: 'popup',
      callback: (response: GoogleCodeResponse) => this.handleGoogleCodeResponse(response),
    });
  }

  signInWithGoogle(): void {
    if (!this.codeClient) {
      this.notificationService.showError('Google login is not ready yet.');
      return;
    }

    this.codeClient.requestCode();
  }

  handleGoogleCodeResponse(response: GoogleCodeResponse): void {
    if (!response?.code) {
      this.notificationService.showError('Google login failed.');
      return;
    }

    this.authService.handleGoogleCallback(response.code).subscribe({
      next: async (data) => {
        const user = await this.authService.storeToken(data.jwtToken);
        const target = this.authService.needsBankInformation(user) ? ['initial-setup'] : [''];
        this.router.navigate(target);
      },
      error: (err): void => {
        console.error(err);
        this.notificationService.showError(err instanceof Error ? err.message : String(err));
      },
    });
  }
}
