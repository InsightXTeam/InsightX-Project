import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Alert } from '../models/alert.model';

import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AlertsApiService {
  private readonly http = inject(HttpClient);
  private readonly BASE = `${environment.apiBaseUrl}/alerts`;

  readonly unseenCount = signal<number>(0);

  fetchUnseenCount(): void {
    this.getAlerts(false).subscribe({
      next: (alerts) => this.unseenCount.set(alerts.length),
      error: () => this.unseenCount.set(0)
    });
  }

  /**
   * GET /api/alerts?seen=true|false
   * Returns all alerts for the current user's company.
   * Pass null to get all.
   */
  getAlerts(seenFilter?: boolean | null): Observable<Alert[]> {
    let params = new HttpParams().set('t', Date.now().toString());
    if (seenFilter !== null && seenFilter !== undefined) {
      params = params.set('seen', String(seenFilter));
    }
    return this.http.get<Alert[]>(this.BASE, { params });
  }

  /**
   * PUT /api/alerts/{id}/seen
   * Marks a single alert as seen by the Owner.
   */
  markAsSeen(alertId: number): Observable<void> {
    return this.http.put<void>(`${this.BASE}/${alertId}/seen`, {});
  }

  /**
   * POST /api/alerts/run
   * Manually triggers anomaly detection (used by Person 2 after report confirm).
   */
  runDetection(payload: {
    departmentId: number;
    kpiName: string;
    currentValue: number;
  }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.BASE}/run`, payload);
  }
}
