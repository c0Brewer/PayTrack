import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../../services/auth/auth-service';
import { Role } from '../../types/exporter';

export const roleGuard = (requiredRole: Role): CanActivateFn => {
  return async () => {
    const router = inject(Router);
    const authService = inject(AuthService);

    // wait for currentUser to be fetched
    const currentUser = await authService.fetchAndStoreUser();
    if (!currentUser) {
      return router.createUrlTree(['/login']);
    }

    if (currentUser.role < requiredRole) {
      // assumes numeric enum, Admin > TeamLead > User
      return router.createUrlTree(['/unauthorized']); // a 403 page
    }

    return true;
  };
};
