import { Routes } from '@angular/router';

import { TeamListComponent } from './components/team/team-list-component/team-list-component';

export const routes: Routes = [
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
