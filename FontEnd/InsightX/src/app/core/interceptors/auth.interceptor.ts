import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, timer, retry } from 'rxjs';
import { AuthService } from '../services/auth.service';

const TRANSIENT_STATUS_CODES = new Set([0, 502, 503, 504]);

const NO_RETRY_PATTERNS = ['/auth/refresh', '/auth/login', '/auth/register'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.accessToken;
  let authReq = req;

  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  const isAuthEndpoint = NO_RETRY_PATTERNS.some(pattern => req.url.includes(pattern));

  return next(authReq).pipe(
    retry({
      count: isAuthEndpoint ? 0 : 2,
      delay: (error, retryCount) => {
        if (error instanceof HttpErrorResponse && TRANSIENT_STATUS_CODES.has(error.status)) {
          return timer(retryCount * 1000);
        }
        throw error;
      }
    }),
    catchError((error) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        if (isAuthEndpoint) {
          authService.logout();
          router.navigate(['/auth/login']);
          return throwError(() => error);
        }

        return authService.refreshToken().pipe(
          switchMap((authResponse) => {
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

