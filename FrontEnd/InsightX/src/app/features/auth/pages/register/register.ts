import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

// Custom validator to check if password and confirmPassword match
export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');

  if (!password || !confirmPassword) return null;

  return password.value === confirmPassword.value ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // States
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly isRegistered = signal(false);

  // Register Form Group
  readonly registerForm: FormGroup = this.fb.group({
    companyName: ['', [Validators.required, Validators.minLength(2)]],
    ownerName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [
      Validators.required, 
      Validators.minLength(8), 
      Validators.pattern(/(?=.*\d)/) // Must contain at least one digit
    ]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  // Getters for validations
  get companyNameControl() {
    return this.registerForm.controls['companyName'];
  }

  get ownerNameControl() {
    return this.registerForm.controls['ownerName'];
  }

  get emailControl() {
    return this.registerForm.controls['email'];
  }

  get passwordControl() {
    return this.registerForm.controls['password'];
  }

  get confirmPasswordControl() {
    return this.registerForm.controls['confirmPassword'];
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(v => !v);
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const registration = {
      companyName: this.registerForm.value.companyName,
      ownerName: this.registerForm.value.ownerName,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password
    };

    this.authService.register(registration).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isRegistered.set(true);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error || 'Registration failed. The company name or email might already be taken.');
      }
    });
  }
}

