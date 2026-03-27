import { Routes } from '@angular/router';

import { LoginComponent } from './components/login/login-component/login-component';
import { TeamListComponent } from './components/team/team-list-component/team-list-component';
import { authGuard } from './guards/auth-guard/auth-guard';
import { guestGuard } from './guards/guest-guard/guest-guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    component: LoginComponent,
  },
  {
    path: 'team',
    canActivate: [authGuard],
    component: TeamListComponent,
  },
  {
    // Fallback. TODO: Replace with proper Component
    path: '**',
    canActivate: [authGuard],
    component: TeamListComponent,
  },
];
