import { Routes } from '@angular/router';

export const routes: Routes = [
  // Report Upload & AI Processing routes
  {
    path: 'reports',
    loadChildren: () => import('./features/reports/reports.routes').then(m => m.routes),
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
