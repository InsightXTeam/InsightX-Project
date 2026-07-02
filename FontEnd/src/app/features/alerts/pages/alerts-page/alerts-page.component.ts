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
  readonly activeTab = signal<FilterTab>('all');

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
    });
  }
}
