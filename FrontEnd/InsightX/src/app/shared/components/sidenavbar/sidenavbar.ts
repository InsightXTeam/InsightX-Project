import { Component, computed, inject, signal, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AlertsApiService } from '../../../features/alerts/services/alerts-api.service';

@Component({
  selector: 'app-sidenavbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidenavbar.html',
  styleUrl: './sidenavbar.css'
})
export class SidenavbarComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly alertsService = inject(AlertsApiService);
  private readonly router = inject(Router);

  @Output() readonly linkClicked = new EventEmitter<void>();

  // States
  readonly isCollapsed = signal(false);
  readonly unseenCount = this.alertsService.unseenCount;

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
    if (r === 'SuperAdmin') return 'Super Admin';
    return r;
  });

  toggleCollapse(): void {
    this.isCollapsed.update(c => !c);
  }

  ngOnInit(): void {
    if (this.role() === 'Owner' || this.role() === 'Manager') {
      this.alertsService.fetchUnseenCount();
    }
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
