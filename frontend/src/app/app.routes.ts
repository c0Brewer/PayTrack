import { Routes } from '@angular/router';

import { UnauthorizedComponent } from './components/general/unauthorized-component/unauthorized-component';
import { LoginComponent } from './components/login/login-component/login-component';
import { ReceiptSubmitComponent } from './components/submission/receipt-submit-component/receipt-submit-component';
import { TeamListComponent } from './components/team/team-list-component/team-list-component';
import { UserManagementComponent } from './components/user-management/user-management-component/user-management-component';
import { authGuard } from './guards/auth-guard/auth-guard';
import { guestGuard } from './guards/guest-guard/guest-guard';
import { roleGuard } from './guards/role-guard/role-guard';
import { Role } from './types/exporter';

/*
 * FYI: For the other devs: The guestGuard and authGuard protect certain routes from unauthorized access.
 *
 * If you add authGuard to a route it means that only a logged in user can access this route (this will be the case for most routes)
 *
 * The guestGuard is a guard which does not allow users who are already logged in to access certain routes (e.g. a logged in user should not be able to go to /login)
 */

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    component: LoginComponent,
  },
  {
    path: 'submit',
    canActivate: [authGuard],
    component: ReceiptSubmitComponent,
  },
  {
    path: 'user',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: UserManagementComponent,
  },
  {
    path: 'team',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: TeamListComponent,
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent,
  },
  {
    // Fallback. TODO: Replace with proper Component
    path: '**',
    canActivate: [authGuard],
    component: TeamListComponent,
  },
];
