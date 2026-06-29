import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidenavbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidenavbar.html',
  styleUrl: './sidenavbar.css'
})
export class SidenavbarComponent {
  // States
  readonly isCollapsed = signal(false);

  // Hardcoded details for isolated testing
  readonly role = computed(() => 'Manager');
  
  readonly userInitials = computed(() => 'MU');

  readonly roleDisplay = computed(() => 'Manager');

  toggleCollapse(): void {
    this.isCollapsed.update(c => !c);
  }

  onLogout(): void {
    console.log('Logout clicked - functionality removed in isolated mode');
  }
}
