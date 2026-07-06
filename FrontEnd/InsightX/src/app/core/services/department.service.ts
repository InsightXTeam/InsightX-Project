import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface DepartmentResponse {
  id: number;
  name: string;
  companyId: number;
  managerName?: string | null;
  managerId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  getDepartments() {
    return this.http.get<DepartmentResponse[]>(`${this.apiBase}/departments`);
  }

  createDepartment(name: string) {
    return this.http.post<DepartmentResponse>(`${this.apiBase}/departments`, { name });
  }

  updateDepartment(id: number, payload: { name: string, managerId: string | null }) {
    return this.http.put<DepartmentResponse>(`${this.apiBase}/departments/${id}`, payload);
  }

  deleteDepartment(id: number) {
    return this.http.delete(`${this.apiBase}/departments/${id}`);
  }
}
