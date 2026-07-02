import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Default → redirect to alerts (Person 3's feature)
  { path: '', redirectTo: 'alerts', pathMatch: 'full' },

  // Auth feature (lazy-loaded — owned by Person 1)
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.authRoutes)
  },

  // Protected shell — all authenticated routes
  {
    path: '',
    canActivate: [authGuard],
    children: [
      // Alerts (Person 3)
      {
        path: 'alerts',
        loadComponent: () =>
          import('./features/alerts/pages/alerts-page/alerts-page.component')
            .then(m => m.AlertsPageComponent),
        title: 'Alerts — InsightX AI'
      },

      // Placeholders for other team members' features
      { path: 'dashboard',  redirectTo: 'alerts' },
      { path: 'reports',    redirectTo: 'alerts' },
      { path: 'users',      redirectTo: 'alerts' },
      { path: 'ai-chat',    redirectTo: 'alerts' },
    ]
  },

  // Fallback
  { path: '**', redirectTo: 'alerts' }
];
