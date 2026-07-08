import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { AlertsApiService } from '../../services/alerts-api.service';
import { Alert } from '../../models/alert.model';
import { AlertCardComponent } from '../../components/alert-card/alert-card.component';

type FilterTab = 'all' | 'unseen' | 'seen';

@Component({
  selector: 'app-alerts-page',
  standalone: true,
  imports: [AlertCardComponent],
  templateUrl: './alerts-page.component.html',
  styleUrls: ['./alerts-page.component.scss']
})
export class AlertsPageComponent implements OnInit {
  private api = inject(AlertsApiService);

  readonly loading   = signal(true);
  readonly error     = signal<string | null>(null);
  readonly allAlerts = signal<Alert[]>([]);
  readonly activeTab = signal<FilterTab>('unseen');

  readonly filtered = computed(() => {
    const tab    = this.activeTab();
    const alerts = this.allAlerts();
    if (tab === 'unseen') return alerts.filter(a => !a.seenByOwner);
    if (tab === 'seen')   return alerts.filter(a =>  a.seenByOwner);
    return alerts;
  });

  readonly unseenCount = computed(() =>
    this.allAlerts().filter(a => !a.seenByOwner).length
  );

  readonly tabs: { key: FilterTab; label: string }[] = [
    { key: 'all',    label: 'All'    },
    { key: 'unseen', label: 'Unseen' },
    { key: 'seen',   label: 'Seen'   },
  ];

  ngOnInit() { this.loadAlerts(); }

  loadAlerts() {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAlerts().subscribe({
      next: alerts => {
        // Newest first
        const sorted = [...alerts].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.allAlerts.set(sorted);
        this.api.unseenCount.set(sorted.filter(a => !a.seenByOwner).length);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load alerts. Please try again.');
        this.loading.set(false);
      }
    });
  }

  setTab(tab: FilterTab) { this.activeTab.set(tab); }

  markSeen(alertId: number) {
    this.api.markAsSeen(alertId).subscribe(() => {
      this.allAlerts.update(alerts =>
        alerts.map(a => a.id === alertId ? { ...a, seenByOwner: true } : a)
      );
      this.api.unseenCount.update(c => Math.max(0, c - 1));
    });
  }

  markAllSeen() {
    if (this.unseenCount() === 0) return;
    this.api.markAllAsSeen().subscribe(() => {
      this.allAlerts.update(alerts =>
        alerts.map(a => ({ ...a, seenByOwner: true }))
      );
      this.api.unseenCount.set(0);
    });
  }

  deleteAlert(alertId: number) {
    this.api.deleteAlert(alertId).subscribe({
      next: () => {
        this.allAlerts.update(alerts => {
          const target = alerts.find(a => a.id === alertId);
          if (target && !target.seenByOwner) {
            this.api.unseenCount.update(c => Math.max(0, c - 1));
          }
          return alerts.filter(a => a.id !== alertId);
        });
        this.api.fetchUnseenCount();
      },
      error: () => {
        this.error.set('Failed to delete alert. Please try again.');
      }
    });
  }
}
