import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  getCompanyMe() {
    return this.http.get<any>(`${this.apiBase}/companies/me`);
  }

  setupCompany(kpis: any[]) {
    return this.http.put(`${this.apiBase}/companies/setup`, { kpis });
  }
}
