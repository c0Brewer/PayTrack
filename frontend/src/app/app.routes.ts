import { Routes } from '@angular/router';

import { LoginComponent } from './components/login/login-component/login-component';
import { TeamListComponent } from './components/team/team-list-component/team-list-component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'team',
    component: TeamListComponent,
  },
  {
    // Fallback. TODO: Replace with proper Component
    path: '**',
    component: TeamListComponent,
  },
];
