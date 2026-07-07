import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AlertDto } from '../../dashboard.service';

@Component({
  selector: 'app-recent-alerts',
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './recent-alerts.html',
  styleUrl: './recent-alerts.css'
})
export class RecentAlerts {
  @Input() alerts: AlertDto[] = [];
}
