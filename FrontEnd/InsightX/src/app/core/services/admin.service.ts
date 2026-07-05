import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface OwnerResponse {
  userId: string;
  ownerName: string;
  ownerEmail: string;
  isActivated: boolean;
  companyId: number;
  companyName: string;
  companyCreatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  getOwners() {
    return this.http.get<OwnerResponse[]>(`${this.apiBase}/superadmin/owners`);
  }

  activateOwner(id: string) {
    return this.http.post(`${this.apiBase}/superadmin/owners/${id}/activate`, {});
  }

  deactivateOwner(id: string) {
    return this.http.post(`${this.apiBase}/superadmin/owners/${id}/deactivate`, {});
  }
}
