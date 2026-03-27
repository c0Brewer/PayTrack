import { isPlatformBrowser } from '@angular/common';
import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Router } from '@angular/router';
import { from, Observable, BehaviorSubject } from 'rxjs';

import { client } from '../../client';
import { GoogleAuthCallbackDto, GoogleAuthResponseDto } from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private platformId = inject(PLATFORM_ID);
  private loggedIn$!: BehaviorSubject<boolean>;

  constructor(private router: Router) {
    this.loggedIn$ = new BehaviorSubject<boolean>(
      isPlatformBrowser(this.platformId) ? this.hasToken() : false,
    );
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
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('jwt');
    }
    this.loggedIn$.next(false);
    this.router.navigate(['login']);
  }

  public isLoggedIn$(): Observable<boolean> {
    return this.loggedIn$.asObservable();
  }

  private hasToken(): boolean {
    if (!isPlatformBrowser(this.platformId)) return false; // SSR: no token
    return localStorage.getItem('jwt') != null;
  }

  public storeToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('jwt', token);
      this.loggedIn$.next(true);
    }
  }
}
