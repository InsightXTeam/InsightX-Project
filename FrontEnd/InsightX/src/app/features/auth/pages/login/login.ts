import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../core/services/toast.service';
import { CompanyService } from '../../../../core/services/company.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly companyService = inject(CompanyService);
  private readonly toastService = inject(ToastService);

  // States
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  togglePasswordVisibility(): void {
    this.showPassword.update(show => !show);
  }

  // Form Group
  readonly loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false]
  });

  // Getters for template validation checks
  get emailControl() {
    return this.loginForm.controls['email'];
  }

  get passwordControl() {
    return this.loginForm.controls['password'];
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const credentials = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        // Successful authentication
        const user = this.authService.currentUser();
        if (!user) {
          this.isLoading.set(false);
          this.errorMessage.set('An error occurred during authentication.');
          return;
        }
        if (response.mustChangePassword) {
          this.isLoading.set(false);
          this.router.navigate(['/profile']);
          this.toastService.show('Please change your password for security reasons');
          return;
        }
        // Smart Redirect based on Role
        if (user.role === 'SuperAdmin') {
          this.router.navigate(['/users/owners']);
        } else if (user.role === 'Owner') {
          // If Owner, check if company setup / onboarding is already completed
          this.companyService.getCompanyMe().pipe(
            catchError((err) => {
              // If fetching company fails, just proceed to onboarding as fallback
              return of(null);
            })
          ).subscribe(company => {
            this.isLoading.set(false);
            if (company && company.departments && company.departments.length > 0) {
              this.router.navigate(['/dashboard']);
            } else {
              this.router.navigate(['/auth/onboarding']);
            }
          });
        } else {
          // Manager or other roles
          this.isLoading.set(false);
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        if (error.status === 401) {
          this.errorMessage.set('Invalid credentials or account is deactivated.');
        } else {
          this.errorMessage.set(error.error || 'Failed to connect to authentication server. Please try again.');
        }
      }
    });
  }
}
