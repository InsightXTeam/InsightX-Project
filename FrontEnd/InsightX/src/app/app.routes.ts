import { Routes } from '@angular/router';
import { isAuthenticatedGuard, roleGuard } from './core/guards/auth.guard';
import { UserListComponent } from './features/users/pages/user-list';
import { OwnersListComponent } from './features/admin/owners-list';
import { ProfileComponent } from './features/profile/pages/profile';
import { RootRedirectComponent } from './core/components/root-redirect';

export const routes: Routes = [
  // Public auth routes (login, register, onboarding)
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.routes)
  },
  
  // Authenticated application routes
  {
    path: 'home',
    component: RootRedirectComponent,
    canActivate: [isAuthenticatedGuard]
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
    canActivate: [isAuthenticatedGuard, roleGuard(['sadmin'])]
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [isAuthenticatedGuard]
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
