import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Alert } from '../models/alert.model';

@Injectable({ providedIn: 'root' })
export class AlertsApiService {
  private readonly http = inject(HttpClient);
  private readonly BASE = '/api/alerts';

  /**
   * GET /api/alerts?seen=true|false
   * Returns all alerts for the current user's company.
   * Pass null to get all.
   */
  getAlerts(seenFilter?: boolean | null): Observable<Alert[]> {
    let params = new HttpParams();
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
    companyId: number;
    departmentId: number;
    kpiName: string;
    currentValue: number;
  }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.BASE}/run`, payload);
  }
}
