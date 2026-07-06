import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { OnboardingComponent } from './pages/onboarding/onboarding';
import { isAuthenticatedGuard, isNotAuthenticatedGuard, roleGuard } from '../../core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [isNotAuthenticatedGuard]
  },
  {
    path: 'register',
    component: RegisterComponent,
    canActivate: [isNotAuthenticatedGuard]
  },
  {
    path: 'onboarding',
    component: OnboardingComponent,
    canActivate: [isAuthenticatedGuard, roleGuard(['Owner'])]
  }
];
