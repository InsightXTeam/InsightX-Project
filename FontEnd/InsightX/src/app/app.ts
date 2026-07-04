import { Component, inject, computed, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidenavbarComponent } from './shared/components/sidenavbar/sidenavbar';
import { AuthService } from './core/services/auth.service';
import { ToastContainerComponent } from './shared/components/toast/toast-container';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule, SidenavbarComponent, ToastContainerComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly authService = inject(AuthService);

  readonly isMobileMenuOpen = signal(false);

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(v => !v);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }

  // Computed details to display at the end of the top navbar
  readonly user = this.authService.currentUser;

  readonly userEmail = computed(() => this.user()?.email || 'Guest');
  readonly userName = computed(() => this.user()?.name || 'Guest');

  readonly roleDisplay = computed(() => {
    const role = this.user()?.role;
    if (!role) return '';
    if (role.toLowerCase() === 'sadmin') return 'Super Admin';
    return role;
  });

  readonly userInitials = computed(() => {
    const name = this.userName();
    if (name === 'Guest') return 'G';
    const parts = name.trim().split(/[\s._-]+/);
    if (parts.length >= 2 && parts[0] && parts[1]) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  });
}
