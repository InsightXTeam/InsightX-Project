import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-root-redirect',
  standalone: true,
  template: `<div style="text-align: center; padding: 50px; color: #64748b;">Redirecting...</div>`
})
export class RootRedirectComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    if (user.role === 'sadmin') {
      this.router.navigate(['/users/owners']);
    } else if (user.role === 'Owner') {
      // Check if owner's company has departments configured yet
      this.http.get<any>(`${environment.apiBaseUrl}/companies/me`).pipe(
        catchError(() => of(null))
      ).subscribe(company => {
        if (company && company.departments && company.departments.length > 0) {
          this.router.navigate(['/departments']);
        } else {
          this.router.navigate(['/auth/onboarding']);
        }
      });
    } else {
      // Manager redirect
      this.router.navigate(['/profile']);
    }
  }
}
