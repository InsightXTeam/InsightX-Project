import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const currentUser = authService.currentUserValue;

  if (currentUser) {
    let headers = req.headers
      .set('X-User-Id', currentUser.id.toString())
      .set('X-Role', currentUser.role)
      .set('X-Company-Id', currentUser.companyId.toString());

    if (currentUser.departmentId !== null) {
      headers = headers.set('X-Department-Id', currentUser.departmentId.toString());
    }

    const authReq = req.clone({ headers });
    return next(authReq);
  }

  return next(req);
};
