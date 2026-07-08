import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-terms-of-service',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './terms-of-service.html',
  styleUrl: './terms-of-service.css'
})
export class TermsOfServiceComponent {
  readonly authService = inject(AuthService);
}
