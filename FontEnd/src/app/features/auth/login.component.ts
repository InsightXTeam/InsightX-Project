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
  public selectedRole: string = 'Owner';
  public selectedDeptId: number = 1;

  constructor(private authService: AuthService, private router: Router) {}

  public login(): void {
    const dept = this.selectedRole === 'Owner' ? null : Number(this.selectedDeptId);
    this.authService.login(this.selectedRole, dept);
    this.router.navigate(['/dashboard']);
  }

  public loginQuick(role: string, deptId: number | null): void {
    this.authService.login(role, deptId);
    this.router.navigate(['/dashboard']);
  }
}
