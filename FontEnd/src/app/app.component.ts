import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { SidenavbarComponent } from './shared/components/sidenavbar/sidenavbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SidenavbarComponent],
  template: `
    @if (auth.isAuth()) {
      <div class="app-shell">
        <app-sidenavbar />
        <main class="main-content">
          <router-outlet />
        </main>
      </div>
    } @else {
      <router-outlet />
    }
  `,
  styles: [`
    :host { display: block; height: 100vh; }
  `]
})
export class AppComponent {
  readonly auth = inject(AuthService);
}
