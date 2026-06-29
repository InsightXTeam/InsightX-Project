import { Component, computed } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidenavbarComponent } from './shared/components/sidenavbar/sidenavbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, SidenavbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  // Hardcoded details for isolated testing
  readonly userEmail = computed(() => 'manager@insightx.com');
  readonly userName = computed(() => 'Manager User');
  readonly roleDisplay = computed(() => 'Manager');
  readonly userInitials = computed(() => 'MU');
}
