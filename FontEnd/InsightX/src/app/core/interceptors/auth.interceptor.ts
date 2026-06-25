import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.accessToken;
  let authReq = req;

  // Attach access token to the headers if present
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error) => {
      // Check if unauthorized
      if (error instanceof HttpErrorResponse && error.status === 401) {
        const url = req.url;
        
        // Prevent infinite loop if authentication endpoints themselves fail with 401
        if (url.includes('/auth/refresh') || url.includes('/auth/login') || url.includes('/auth/register')) {
          authService.logout();
          router.navigate(['/auth/login']);
          return throwError(() => error);
        }

        // Try to obtain a new access token using refresh token
        return authService.refreshToken().pipe(
          switchMap((authResponse) => {
            // Re-clone request with the new access token and retry
            const retriedReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${authResponse.accessToken}`
              }
            });
            return next(retriedReq);
          }),
          catchError((refreshErr) => {
            // If token refresh fails, log out and redirect to login page
            authService.logout();
            router.navigate(['/auth/login']);
            return throwError(() => refreshErr);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
