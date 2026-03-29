import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, from, Observable } from 'rxjs';

import { client } from '../../client';
import { GoogleAuthCallbackDto, GoogleAuthResponseDto } from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly loggedInSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  public loggedIn$ = this.loggedInSubject.asObservable();

  constructor(private readonly router: Router) {
    this.checkExpiryOnStartup();
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

  public logout(): void {
    localStorage.removeItem('jwt');
    this.loggedInSubject.next(false);
    this.router.navigate(['login']);
  }

  public isLoggedIn(): boolean {
    return this.hasValidToken();
  }

  public storeToken(token: string): void {
    localStorage.setItem('jwt', token);
    this.loggedInSubject.next(true);
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
