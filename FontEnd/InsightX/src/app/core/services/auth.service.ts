import { Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface User {
  id: string;
  email: string;
  name: string;
  role: 'sadmin' | 'Owner' | 'Manager' | string;
  companyId: number;
  departmentId: number | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Mocked user for isolated testing of Report feature
  readonly currentUser = signal<User | null>({
    id: 'mock-user-1',
    email: 'test@insightx.com',
    name: 'Test Manager',
    role: 'Manager',
    companyId: 1,
    departmentId: 1
  });

  constructor() {}

  get accessToken(): string | null {
    return 'mock-token';
  }

  get refreshTokenValue(): string | null {
    return 'mock-refresh-token';
  }

  isAuthenticated(): boolean {
    return true; // Always authenticated in isolated mode
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return of({ accessToken: 'mock-token', refreshToken: 'mock-refresh-token' });
  }

  register(registration: { companyName: string; ownerName: string; email: string; password: string }): Observable<AuthResponse> {
    return of({ accessToken: 'mock-token', refreshToken: 'mock-refresh-token' });
  }

  refreshToken(): Observable<AuthResponse> {
    return of({ accessToken: 'mock-token', refreshToken: 'mock-refresh-token' });
  }

  logout(): void {
    console.log('Mock logout called');
  }
}
