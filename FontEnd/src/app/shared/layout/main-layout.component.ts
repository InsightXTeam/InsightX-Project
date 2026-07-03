import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SideNavbarComponent } from '../components/sidenavbar/sidenavbar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, SideNavbarComponent],
  template: `
    <div class="layout-wrapper d-flex">
      <app-sidenavbar></app-sidenavbar>
      <main class="main-content flex-grow-1 p-4">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .layout-wrapper { min-height: 100vh; }
    .main-content {
      margin-left: 260px;
      background: var(--bg-primary);
      min-height: 100vh;
    }
  `]
})
export class MainLayoutComponent {}
