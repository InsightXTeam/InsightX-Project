import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Protects routes that require an authenticated user.
 * Redirects to /auth/login if no valid session exists.
 */
export const authGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuth()) return true;

  router.navigate(['/auth/login']);
  return false;
};
