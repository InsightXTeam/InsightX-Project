import { Routes } from '@angular/router';
import { isAuthenticatedGuard, roleGuard } from './core/guards/auth.guard';
import { UserListComponent } from './features/users/pages/user-list';
import { OwnersListComponent } from './features/admin/owners-list';
import { ProfileComponent } from './features/profile/pages/profile';
import { LandingPageComponent } from './features/landing/pages/landing-page/landing-page';

export const routes: Routes = [
  // Public auth routes (login, register, onboarding)
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.routes)
  },
  
  // Guest landing page & smart auth redirect
  {
    path: 'home',
    component: LandingPageComponent
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard-page/dashboard-page').then(m => m.DashboardPage),
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner', 'Manager'])]
  },
  {
    path: 'departments',
    loadChildren: () => import('./features/departments/departments.routes').then(m => m.routes),
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner'])]
  },
  {
    path: 'kpis',
    loadChildren: () => import('./features/kpis/kpis.routes').then(m => m.routes),
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner'])]
  },
  {
    path: 'users',
    component: UserListComponent,
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner'])]
  },
  {
    path: 'users/owners',
    component: OwnersListComponent,
    canActivate: [isAuthenticatedGuard, roleGuard(['SuperAdmin'])]
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [isAuthenticatedGuard]
  },
  {
    path: 'reports',
    loadChildren: () => import('./features/reports/reports.routes').then(m => m.routes),
    canActivate: [isAuthenticatedGuard]
  },
  {
    path: 'alerts',
    loadChildren: () => import('./features/alerts/alerts.routes').then(m => m.routes),
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner', 'Manager'])]
  },
  {
    path: 'chat',
    loadComponent: () => import('./features/chat/chat-page/chat-page.component').then(m => m.ChatPageComponent),
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner', 'Manager'])]
  },
  {
    path: 'privacy',
    loadComponent: () => import('./features/legal/pages/privacy-policy/privacy-policy').then(m => m.PrivacyPolicyComponent)
  },
  {
    path: 'terms',
    loadComponent: () => import('./features/legal/pages/terms-of-service/terms-of-service').then(m => m.TermsOfServiceComponent)
  },
  
  // Wildcards & Default Redirects
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'home'
  },
  {
    path: '**',
    redirectTo: 'home'
  }
];
