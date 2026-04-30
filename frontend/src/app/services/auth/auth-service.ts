import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, from, Observable } from 'rxjs';

import { client } from '../../client';
import { GoogleAuthCallbackDto, GoogleAuthResponseDto, UserDto } from '../../types/exporter';
import { NotificationService } from '../notification/notification-service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly loggedInSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  private readonly currentUserSubject = new BehaviorSubject<UserDto | null>(null);

  public loggedIn$ = this.loggedInSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private readonly router: Router,
    private readonly notificationService: NotificationService,
  ) {
    this.checkExpiryOnStartup();
    if (this.hasValidToken()) {
      this.initCurrentUser();
    }
  }

  private async initCurrentUser(): Promise<void> {
    try {
      await this.fetchAndStoreUser();
    } catch (error) {
      this.notificationService.showError('Error while loading User' + error);
      this.logout();
    }
  }

  private checkExpiryOnStartup(): void {
    const token = localStorage.getItem('jwt');
    if (token && this.isTokenExpired(token)) {
      this.logout(); // cleans up and redirects
    }
  }

  loadGoogleScript(): Promise<void> {
    return new Promise((resolve) => {
      if (globalThis.window.google) {
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.onload = (): void => resolve();
      document.body.appendChild(script);
    });
  }

  public handleGoogleCallback(code: string): Observable<GoogleAuthResponseDto> {
    const callbackDto: GoogleAuthCallbackDto = { code };
    const promise = client
      .POST('/api/v1/auth/google', { params: {}, body: callbackDto })
      .then(({ data, error }) => {
        if (error) throw new Error(error.detail ?? 'Unexpected Error');
        return data;
      });
    return from(promise);
  }

  public async refreshUser(): Promise<UserDto> {
    return await this.fetchAndStoreUser();
  }

  public getCurrentUser(): Observable<UserDto | null> {
    return this.currentUser$;
  }

  public async fetchAndStoreUser(): Promise<UserDto> {
    const { data, error } = await client.GET('/api/v1/auth/currentuser', { params: {} });

    if (error) {
      throw new Error(error.detail ?? 'Unexpected Error');
    }

    this.currentUserSubject.next(data);
    return data;
  }

  public logout(): void {
    localStorage.removeItem('jwt');
    this.loggedInSubject.next(false);
    this.currentUserSubject.next(null);
    this.router.navigate(['login']);
  }

  public isLoggedIn(): boolean {
    return this.hasValidToken();
  }

  public async storeToken(token: string): Promise<UserDto> {
    localStorage.setItem('jwt', token);
    this.loggedInSubject.next(true);
    return await this.fetchAndStoreUser();
  }

  public needsBankInformation(user: UserDto | null): boolean {
    return !!user && !user.hasBankInformation && !user.bankInformationSkipped;
  }

  public async skipBankInformation(): Promise<UserDto> {
    const { data, error } = await client.POST('/api/v1/bankaccount/onboarding/skip', {
      params: {},
    });

    if (error) {
      throw new Error(error.detail ?? 'Unexpected Error');
    }

    this.currentUserSubject.next(data);
    return data;
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 < Date.now();
    } catch {
      return true; // treat malformed tokens as expired
    }
  }

  private hasValidToken(): boolean {
    const token = localStorage.getItem('jwt');
    return token != null && !this.isTokenExpired(token);
  }

  public getToken(): string | null {
    if (this.hasValidToken()) {
      return localStorage.getItem('jwt');
    }

    return null;
  }
}
