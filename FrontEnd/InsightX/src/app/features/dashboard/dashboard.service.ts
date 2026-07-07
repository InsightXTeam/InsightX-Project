import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface DashboardKpiDto {
  name: string;
  currentValue: number;
  threshold: number;
  unit: string;
  status: string;
  thresholdDirection: string;
}

export interface DashboardTrendDto {
  kpiName: string;
  values: number[];
  labels: string[];
}

export interface DashboardDepartmentPerformanceDto {
  departmentId: number;
  departmentName: string;
  goodKPIsCount: number;
  warningKPIsCount: number;
  criticalKPIsCount: number;
  overallStatus: string;
}

export interface AlertDto {
  id: number;
  departmentId: number;
  kpiName: string;
  message: string;
  recommendation: string;
  currentValue: number;
  threshold: number;
  alertType: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/dashboard`;

  getKpisSummary() {
    return this.http.get<DashboardKpiDto[]>(`${this.apiUrl}/kpis`);
  }

  getTrends() {
    return this.http.get<DashboardTrendDto[]>(`${this.apiUrl}/trends`);
  }

  getDepartmentsPerformance() {
    return this.http.get<DashboardDepartmentPerformanceDto[]>(`${this.apiUrl}/departments`);
  }

  getRecentAlerts() {
    return this.http.get<AlertDto[]>(`${this.apiUrl}/alerts/recent`);
  }
}
