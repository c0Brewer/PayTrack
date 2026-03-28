import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, from, Observable } from 'rxjs';

import { client } from '../../client';
import { GoogleAuthCallbackDto, GoogleAuthResponseDto } from '../../types/exporter';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private loggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public loggedIn$ = this.loggedInSubject.asObservable();

  constructor(private router: Router) {}

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
    return this.hasToken();
  }

  public storeToken(token: string): void {
    localStorage.setItem('jwt', token);
    this.loggedInSubject.next(true);
  }

  private hasToken(): boolean {
    return localStorage.getItem('jwt') != null;
  }
}
