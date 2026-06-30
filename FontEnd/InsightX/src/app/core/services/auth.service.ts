import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBaseUrl;

  // Signal for the current user state
  readonly currentUser = signal<User | null>(null);

  constructor() {
    this.loadUserFromStorage();
  }

  get accessToken(): string | null {
    return localStorage.getItem('insightx_access_token');
  }

  get refreshTokenValue(): string | null {
    return localStorage.getItem('insightx_refresh_token');
  }

  isAuthenticated(): boolean {
    return this.currentUser() !== null;
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBase}/auth/login`, credentials).pipe(
      tap(response => this.handleAuthentication(response, credentials.email))
    );
  }

  register(registration: { companyName: string; ownerName: string; email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBase}/auth/register`, registration).pipe(
      tap(response => this.handleAuthentication(response, registration.email))
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const accessToken = this.accessToken;
    const refreshToken = this.refreshTokenValue;

    if (!accessToken || !refreshToken) {
      this.logout();
      return throwError(() => new Error('No tokens available for refresh'));
    }

    return this.http.post<AuthResponse>(`${this.apiBase}/auth/refresh`, {
      accessToken,
      refreshToken
    }).pipe(
      tap(response => {
        // Maintain the current user email when renewing tokens
        const currentEmail = this.currentUser()?.email || '';
        this.handleAuthentication(response, currentEmail);
      }),
      catchError(error => {
        // If refresh fails (e.g. token expired/invalid), force logout
        this.logout();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    const token = this.accessToken;
    localStorage.removeItem('insightx_access_token');
    localStorage.removeItem('insightx_refresh_token');
    localStorage.removeItem('insightx_user_email');
    this.currentUser.set(null);

    if (token) {
      // Try to call logout API, ignore errors
      this.http.post(`${this.apiBase}/auth/logout`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      }).subscribe({
        error: () => {} // Silent catch
      });
    }
  }

  private handleAuthentication(response: AuthResponse, email: string): void {
    localStorage.setItem('insightx_access_token', response.accessToken);
    localStorage.setItem('insightx_refresh_token', response.refreshToken);
    if (email) {
      localStorage.setItem('insightx_user_email', email);
    }
    
    const user = this.decodeToken(response.accessToken, email);
    this.currentUser.set(user);
  }

  private loadUserFromStorage(): void {
    const token = this.accessToken;
    const email = localStorage.getItem('insightx_user_email') || '';
    if (token) {
      const user = this.decodeToken(token, email);
      if (user) {
        this.currentUser.set(user);
      } else {
        this.logout();
      }
    }
  }

  private decodeToken(token: string, email: string): User | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      
      const payload = parts[1];
      const decodedPayload = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      const claims = JSON.parse(decodedPayload);

      // Extract claims. The .NET claims mapper can sometimes result in URI keys for sub and role.
      const id = claims['sub'] || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      const role = claims['role'] || claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const companyId = parseInt(claims['CompanyId'], 10);
      const name = claims['name'] || claims['unique_name'] || claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || email.split('@')[0] || '';
      
      let departmentId: number | null = null;
      if (claims['DepartmentId']) {
        const parsedDept = parseInt(claims['DepartmentId'], 10);
        if (!isNaN(parsedDept)) {
          departmentId = parsedDept;
        }
      }

      // Check token expiry
      if (claims.exp) {
        const expiryTime = claims.exp * 1000;
        if (Date.now() >= expiryTime) {
          return null; // Expired
        }
      }

      // Validate issued-at claim to reject tokens issued in the future (clock skew protection)
      if (claims.iat) {
        const issuedAt = claims.iat * 1000;
        const maxFutureTolerance = 60_000; // Allow 60 seconds of clock skew
        if (issuedAt > Date.now() + maxFutureTolerance) {
          return null; // Token issued in the future — possible forgery
        }
      }

      return {
        id,
        email,
        name,
        role,
        companyId,
        departmentId
      };
    } catch (e) {
      return null;
    }
  }
}
