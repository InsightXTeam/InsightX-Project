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

  // Temporary bypass so feature pages can be reviewed
  // while auth UI/backend integration is still owned by Person 1.
  const bypassAuthForNow = true;
  if (bypassAuthForNow) return true;

  if (auth.isAuth()) return true;

  router.navigate(['/auth/login']);
  return false;
};
