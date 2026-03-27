import { isPlatformBrowser } from '@angular/common';
import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  console.log('Trigger2');
  // if (!isPlatformBrowser(platformId)) {
  //   // On SSR, redirect to login — browser will re-evaluate after hydration
  //   return router.createUrlTree(['/login']);
  // }

  const token = localStorage.getItem('jwt');
  console.log('Logged: ', token);
  if (!token) {
    return router.createUrlTree(['/login']);
  }
  return true;
};
