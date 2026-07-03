import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  public email = 'owner@insightx.com';
  public password = 'Password123!';
  public errorMessage = '';
  public isLoading = false;

  constructor(private authService: AuthService, private router: Router) {}

  public login(): void {
    this.errorMessage = '';
    this.isLoading = true;

    this.authService.loginWithCredentials(this.email, this.password).subscribe(response => {
      this.isLoading = false;
      if (response) {
        this.router.navigate(['/dashboard']);
      } else {
        this.errorMessage = 'Invalid email or password.';
      }
    });
  }

  public loginQuick(email: string): void {
    this.email = email;
    this.password = 'Password123!';
    this.login();
  }
}
