import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../../services/auth/auth-service';

export const activeGuard: CanActivateFn = async () => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const currentUser = await authService.fetchAndStoreUser();
  if (!currentUser) {
    return router.createUrlTree(['/login']);
  }

  if (!currentUser.isActive) {
    return router.createUrlTree(['/unauthorized']);
  }

  return true;
};
