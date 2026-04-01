import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, from, Observable } from 'rxjs';

import { client } from '../../client';
import { GoogleAuthCallbackDto, GoogleAuthResponseDto, UserDto } from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly loggedInSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  private readonly currentUserSubject = new BehaviorSubject<UserDto | null>(null);

  public loggedIn$ = this.loggedInSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private readonly router: Router) {
    this.checkExpiryOnStartup();
    if (this.hasValidToken()) {
      if (this.hasValidToken()) {
        this.initCurrentUser();
      }
    }
  }

  private async initCurrentUser(): Promise<void> {
    try {
      await this.fetchAndStoreUser();
    } catch (e) {
      console.error(e);
      this.logout();
    }
  }

  private checkExpiryOnStartup(): void {
    const token = localStorage.getItem('jwt');
    if (token && this.isTokenExpired(token)) {
      this.logout(); // cleans up and redirects
    }
  }

  public handleGoogleCallback(idToken: string): Observable<GoogleAuthResponseDto> {
    const callbackDto: GoogleAuthCallbackDto = { idToken };
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

  public storeToken(token: string): void {
    localStorage.setItem('jwt', token);
    this.loggedInSubject.next(true);
    this.fetchAndStoreUser();
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
}
