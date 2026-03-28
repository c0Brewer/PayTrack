import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../../services/auth/auth-service';

export const guestGuard: CanActivateFn = () => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const isLoggedIn = authService.isLoggedIn();
  if (isLoggedIn) {
    return router.createUrlTree(['']); // already logged in, redirect away
  }
  return true;
};
