import { Routes } from '@angular/router';
import { isAuthenticatedGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Authenticated application routes
  {
    path: 'reports',
    loadChildren: () => import('./features/reports/reports.routes').then(m => m.routes),
    canActivate: [isAuthenticatedGuard]
  },
  
  // Wildcards & Default Redirects
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'reports'
  },
  {
    path: '**',
    redirectTo: 'reports'
  }
];
