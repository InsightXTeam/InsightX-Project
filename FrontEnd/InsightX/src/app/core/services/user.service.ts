import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface UserResponse {
  id: string;
  name: string;
  email: string;
  role: string;
  departmentId: number | null;
  departmentName: string | null;
  isDeleted: boolean;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  getUsers() {
    return this.http.get<UserResponse[]>(`${this.apiBase}/users`);
  }

  inviteManager(payload: any) {
    return this.http.post(`${this.apiBase}/users/invite`, payload);
  }

  deleteUser(id: string) {
    return this.http.delete(`${this.apiBase}/users/${id}`);
  }

  restoreUser(id: string) {
    return this.http.post(`${this.apiBase}/users/${id}/restore`, {});
  }

  updateUserDepartment(id: string, departmentId: number | null) {
    return this.http.put(`${this.apiBase}/users/${id}/department`, { departmentId });
  }

  changePassword(payload: any) {
    return this.http.post(`${this.apiBase}/users/change-password`, payload);
  }
}
