import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { catchError, map, of } from 'rxjs';

export const isAuthenticatedGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If already authenticated (unexpired JWT), allow navigation
  if (authService.isAuthenticated()) {
    return true;
  }

  // If access token is expired or missing, but refresh token is available, attempt silent refresh
  if (authService.refreshTokenValue) {
    return authService.refreshToken().pipe(
      map(() => true), // Silent refresh succeeded, allow navigation
      catchError(() => {
        // Silent refresh failed (refresh token expired/invalid), redirect to login
        router.navigate(['/auth/login']);
        return of(false);
      })
    );
  }

  // No tokens available, redirect to login
  router.navigate(['/auth/login']);
  return false;
};

export const isNotAuthenticatedGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  // If already authenticated, redirect to main application landing
  router.navigate(['/']);
  return false;
};

/**
 * Functional Role Guard that verifies if the logged-in user possesses one of the allowed roles.
 * Matches case-insensitively to prevent configuration mismatch.
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const user = authService.currentUser();

    if (user && allowedRoles.some(role => role.toLowerCase() === user.role.toLowerCase())) {
      return true;
    }

    // Role check failed -> redirect to the root redirect component to place them on their safe default home page
    router.navigate(['/home']);
    return false;
  };
};
