import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DashboardAlert,
  DepartmentSummary,
  KpiCard,
  TrendPoint
} from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly baseUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getKpis(): Observable<KpiCard[]> {
    return this.http.get<KpiCard[]>(`${this.baseUrl}/kpis`);
  }

  getTrends(kpiName: string): Observable<TrendPoint[]> {
    return this.http.get<TrendPoint[]>(`${this.baseUrl}/trends`, {
      params: { kpiName }
    });
  }

  getDepartments(): Observable<DepartmentSummary[]> {
    return this.http.get<DepartmentSummary[]>(`${this.baseUrl}/departments`);
  }

  getRecentAlerts(): Observable<DashboardAlert[]> {
    return this.http.get<DashboardAlert[]>(`${this.baseUrl}/alerts/recent`);
  }
}
