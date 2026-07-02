import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Alert } from '../../models/alert.model';

@Component({
  selector: 'app-alert-card',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  templateUrl: './alert-card.component.html',
  styleUrls: ['./alert-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AlertCardComponent {
  @Input({ required: true }) alert!: Alert;
  @Output() markSeen = new EventEmitter<number>();

  get badge(): string {
    if (this.alert.seenByOwner) return 'SEEN';
    if (this.alert.alertType === 'MonthlyReminder') return 'REMINDER';
    const ratio = this.alert.currentValue / this.alert.threshold;
    return ratio >= 1.3 ? 'CRITICAL' : 'WARNING';
  }

  get badgeClass(): string {
    const map: Record<string, string> = {
      CRITICAL: 'badge--critical',
      WARNING:  'badge--warning',
      SEEN:     'badge--seen',
      REMINDER: 'badge--reminder',
    };
    return map[this.badge];
  }

  get borderClass(): string {
    if (this.alert.seenByOwner) return 'card--seen';
    if (this.alert.alertType === 'MonthlyReminder') return 'card--reminder';
    const ratio = this.alert.currentValue / this.alert.threshold;
    return ratio >= 1.3 ? 'card--critical' : 'card--warning';
  }

  get currentLabel(): string {
    return this.alert.alertType === 'MonthlyReminder' ? '' : 'Current Value';
  }

  get thresholdLabel(): string {
    return this.alert.alertType === 'MonthlyReminder' ? '' : 'Threshold';
  }

  onMarkSeen(): void {
    this.markSeen.emit(this.alert.id);
  }
}
