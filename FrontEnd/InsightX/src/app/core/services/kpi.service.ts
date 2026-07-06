import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface KpiResponse {
  id: number;
  name: string;
  threshold: number;
  unit: string;
  companyId: number;
  alertPercentageDiff: number;
  trendMonthsCount: number;
  thresholdDirection: number;
}

@Injectable({ providedIn: 'root' })
export class KpiService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  getKpis() {
    return this.http.get<KpiResponse[]>(`${this.apiBase}/kpis`);
  }

  createKpi(payload: any) {
    return this.http.post<KpiResponse>(`${this.apiBase}/kpis`, payload);
  }

  updateKpi(id: number, payload: any) {
    return this.http.put<KpiResponse>(`${this.apiBase}/kpis/${id}`, payload);
  }

  deleteKpi(id: number) {
    return this.http.delete(`${this.apiBase}/kpis/${id}`);
  }
}
