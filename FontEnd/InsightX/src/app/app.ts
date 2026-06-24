import { Component, inject, computed } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidenavbarComponent } from './shared/components/sidenavbar/sidenavbar';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule, SidenavbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly authService = inject(AuthService);

  // Computed details to display at the end of the top navbar
  readonly user = this.authService.currentUser;

  readonly userEmail = computed(() => this.user()?.email || 'Guest');

  readonly roleDisplay = computed(() => {
    const role = this.user()?.role;
    if (!role) return '';
    if (role.toLowerCase() === 'sadmin') return 'Super Admin';
    return role;
  });

  readonly userInitials = computed(() => {
    const email = this.userEmail();
    if (email === 'Guest') return 'G';
    return email.split('@')[0].substring(0, 2).toUpperCase();
  });
}
