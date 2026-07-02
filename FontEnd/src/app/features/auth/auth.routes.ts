import { Routes } from '@angular/router';

// Placeholder — auth screens to be implemented by Person 1
export const authRoutes: Routes = [
  { path: 'login',    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  { path: '',         redirectTo: 'login', pathMatch: 'full' }
];
