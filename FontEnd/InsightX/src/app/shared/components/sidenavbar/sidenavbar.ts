import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidenavbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidenavbar.html',
  styleUrl: './sidenavbar.css'
})
export class SidenavbarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // States
  readonly isCollapsed = signal(false);

  // Computed User Details
  readonly user = this.authService.currentUser;
  readonly role = computed(() => this.user()?.role || '');
  
  readonly userInitials = computed(() => {
    const name = this.user()?.name || '';
    if (name) {
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

  toggleCollapse(): void {
    this.isCollapsed.update(c => !c);
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
