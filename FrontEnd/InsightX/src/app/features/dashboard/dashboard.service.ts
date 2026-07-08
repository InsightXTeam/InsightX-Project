import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface DashboardKpiDto {
  name: string;
  currentValue: number;
  threshold: number;
  unit: string;
  status: "Good" | "Warning" | "Critical";
  thresholdDirection: "Minimum Target" | "Maximum Limit";
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
  overallStatus: "Good" | "Warning" | "Critical";
}

export interface AlertDto {
  id: number;
  kpiName: string;
  currentValue: number;
  threshold: number;
  message: string;
  recommendation: string;
  seenByOwner: boolean;
  createdAt: string;
  alertType: number;
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

  getTrends(months: number = 6) {
    return this.http.get<DashboardTrendDto[]>(`${this.apiUrl}/trends?months=${months}`);
  }

  getDepartmentsPerformance() {
    return this.http.get<DashboardDepartmentPerformanceDto[]>(`${this.apiUrl}/departments`);
  }

  getRecentAlerts() {
    return this.http.get<AlertDto[]>(`${this.apiUrl}/alerts/recent`);
  }
}
