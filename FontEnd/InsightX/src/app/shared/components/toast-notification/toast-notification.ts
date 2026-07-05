import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-toast-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-notification.html',
  styleUrls: ['./toast-notification.css'],
})
export class ToastNotificationComponent {
  @Input() open = false;
  @Input() message = '';
  @Input() type: 'success' | 'error' | 'info' = 'success';
}
