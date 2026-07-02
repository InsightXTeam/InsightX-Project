import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService, UserContext } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidenavbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidenavbar.component.html',
  styleUrls: ['./sidenavbar.component.css']
})
export class SideNavbarComponent implements OnInit {
  public currentUser: UserContext | null = null;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  getRoleBadgeClass(): string {
    if (!this.currentUser) return '';
    return this.currentUser.role === 'Owner' ? 'bg-primary' : 'bg-secondary';
  }

  getRoleLabel(): string {
    if (!this.currentUser) return '';
    if (this.currentUser.role === 'Owner') return 'Owner';
    return this.currentUser.departmentId === 1 ? 'Prod Manager' :
           this.currentUser.departmentId === 2 ? 'Quality Manager' : 'HR Manager';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
