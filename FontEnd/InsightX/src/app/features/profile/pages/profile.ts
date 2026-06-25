import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const newPass = control.get('newPassword');
  const confirmPass = control.get('confirmPassword');
  
  if (!newPass || !confirmPass) return null;
  return newPass.value !== confirmPass.value ? { mismatch: true } : null;
};

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  // Status Signals
  readonly isLoading = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);

  // Compute Current User Claims
  readonly user = this.authService.currentUser;
  readonly userId = computed(() => this.user()?.id || 'Unknown');
  readonly email = computed(() => this.user()?.email || 'No email associated');
  readonly userName = computed(() => this.user()?.name || 'No name associated');
  readonly role = computed(() => this.user()?.role || 'Guest');
  readonly companyId = computed(() => this.user()?.companyId || 0);
  readonly departmentId = computed(() => this.user()?.departmentId || null);

  readonly userInitials = computed(() => {
    const name = this.userName();
    if (name && name !== 'No name associated') {
      const parts = name.trim().split(/[\s._-]+/);
      if (parts.length >= 2 && parts[0] && parts[1]) {
        return (parts[0][0] + parts[1][0]).toUpperCase();
      }
      return name.substring(0, 2).toUpperCase();
    }
    return 'U';
  });

  readonly roleDisplay = computed(() => {
    const r = this.role();
    if (r === 'sadmin') return 'Super Admin';
    return r;
  });

  // Password FormGroup
  readonly passwordForm: FormGroup = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [
      Validators.required, 
      Validators.minLength(8), 
      Validators.pattern(/(?=.*\d)/) // Requires at least one digit
    ]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  // Getters for form validations
  get currentPasswordControl() {
    return this.passwordForm.controls['currentPassword'];
  }

  get newPasswordControl() {
    return this.passwordForm.controls['newPassword'];
  }

  get confirmPasswordControl() {
    return this.passwordForm.controls['confirmPassword'];
  }

  onSubmit(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const payload = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.http.post(`${this.apiBase}/users/change-password`, payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.successMessage.set('Password updated successfully.');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error || 'Failed to change password. Make sure current password is correct.');
      }
    });
  }
}
