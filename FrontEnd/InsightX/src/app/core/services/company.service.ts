import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface CompanyDepartment {
  id: number;
  name: string;
}

export interface CompanyMeResponse {
  id?: number;
  name?: string | null;
  companyName?: string | null;
  departments?: CompanyDepartment[];
}

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  getCompanyMe() {
    return this.http.get<CompanyMeResponse>(`${this.apiBase}/companies/me`);
  }

  setupCompany(kpis: any[]) {
    return this.http.put(`${this.apiBase}/companies/setup`, { kpis });
  }
}
