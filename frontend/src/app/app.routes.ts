import { Routes } from '@angular/router';

import { CostCentreDetailComponent } from './components/cost-centre/cost-centre-detail-component/cost-centre-detail-component';
import { CostCentreManagementComponent } from './components/cost-centre/cost-centre-management-component/cost-centre-management-component';
import { UnauthorizedComponent } from './components/general/unauthorized-component/unauthorized-component';
import { HomeComponent } from './components/home/home-component/home-component';
import { InitialLoginBankAccountComponent } from './components/inital-login-bank-account/initial-login-bank-account-component/initial-login-bank-account-component';
import { LoginComponent } from './components/login/login-component/login-component';
import { BankStatementDetailComponent } from './components/payment-requests/bank-statement-detail-component/bank-statement-detail-component';
import { TeamRequestAdminDetailComponent } from './components/payment-requests/request-by-team/admin-view/admin-detail-component/admin-detail-component';
import { TeamRequestAdminOverviewComponent } from './components/payment-requests/request-by-team/admin-view/admin-overview-component/admin-overview-component';
import { PaymentRequestByTeamComponent } from './components/payment-requests/request-by-team/submission-component/submission-component';
import { TeamRequestTeamDetailPageComponent } from './components/payment-requests/request-by-team/team-view/team-detail-page-component/team-detail-page-component';
import { TeamRequestTeamOverviewComponent } from './components/payment-requests/request-by-team/team-view/team-overview-component/team-overview-component';
import { RequestDetailComponent } from './components/payment-requests/request-by-user/admin-view/admin-detail-component/admin-detail-component';
import { AdminInvoicesOverviewComponent } from './components/payment-requests/request-by-user/admin-view/admin-overview-component/admin-overview-component';
import { ReceiptSubmitComponent } from './components/payment-requests/request-by-user/submission-component/submission-component';
import { MyInvoiceDetailComponent } from './components/payment-requests/request-by-user/user-view/user-detail-component/user-detail-component';
import { UserInvoicesOverviewComponent } from './components/payment-requests/request-by-user/user-view/user-overview-component/user-overview-component';
import { SeasonManagementComponent } from './components/season/season-management-component/season-management-component';
import { SettingsComponent } from './components/settings/settings-component/settings-component';
import { TeamDetailComponent } from './components/team/team-detail-component/team-detail-component';
import { TeamManagementComponent } from './components/team/team-management-component/team-management-component';
import { UserManagementComponent } from './components/user-management/user-management-component/user-management-component';
import { activeGuard } from './guards/active-guard/active-guard';
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
    path: 'my-invoices',
    canActivate: [authGuard],
    component: UserInvoicesOverviewComponent,
  },
  {
    path: 'my-invoices/:id',
    canActivate: [authGuard],
    component: MyInvoiceDetailComponent,
  },
  {
    path: 'submit',
    canActivate: [authGuard, activeGuard],
    component: ReceiptSubmitComponent,
  },
  {
    path: 'requests',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: AdminInvoicesOverviewComponent,
  },
  {
    path: 'bank-statement-upload',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: BankStatementDetailComponent,
  },
  {
    path: 'requests/:id',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: RequestDetailComponent,
  },
  {
    path: 'user',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: UserManagementComponent,
  },
  {
    path: 'team',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: TeamManagementComponent,
  },
  {
    path: 'team/:id',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: TeamDetailComponent,
  },
  {
    path: 'initial-setup',
    canActivate: [authGuard],
    component: InitialLoginBankAccountComponent,
  },
  {
    path: 'cost-centre',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: CostCentreManagementComponent,
  },
  {
    path: 'season',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: SeasonManagementComponent,
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    component: SettingsComponent,
  },
  {
    path: 'cost-centre/:id',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: CostCentreDetailComponent,
  },
  {
    path: 'create-payment-request',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: PaymentRequestByTeamComponent,
  },
  {
    path: 'payment-requests-by-team',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: TeamRequestAdminOverviewComponent,
  },
  {
    path: 'payment-requests-by-team/:id',
    canActivate: [authGuard, roleGuard(Role.ADMIN)],
    component: TeamRequestAdminDetailComponent,
  },
  {
    path: 'my-team-requests',
    canActivate: [authGuard],
    component: TeamRequestTeamOverviewComponent,
  },
  {
    path: 'my-team-requests/:id',
    canActivate: [authGuard],
    component: TeamRequestTeamDetailPageComponent,
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent,
  },
  {
    path: '',
    canActivate: [authGuard],
    component: HomeComponent,
  },
  {
    path: '**',
    redirectTo: '',
  },
];
