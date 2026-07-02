import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';

export interface AuthUser {
  id: number;
  name: string;
  email: string;
  role: 'OWNER' | 'MANAGER' | 'EMPLOYEE';
  companyId: number;
  avatarUrl?: string;
}

interface LoginPayload { email: string; password: string; }
interface LoginResponse { token: string; user: AuthUser; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = '/api';
  private readonly TOKEN_KEY = 'ix_token';
  private readonly USER_KEY  = 'ix_user';

  // Reactive state
  private _user = signal<AuthUser | null>(this.loadUser());
  readonly user   = this._user.asReadonly();
  readonly isAuth = computed(() => !!this._user());
  readonly companyId = computed(() => this._user()?.companyId ?? null);

  constructor(private http: HttpClient, private router: Router) {}

  login(payload: LoginPayload) {
    return this.http.post<LoginResponse>(`${this.API}/auth/login`, payload).pipe(
      tap(res => this.persist(res.token, res.user))
    );
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._user.set(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private persist(token: string, user: AuthUser) {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  private loadUser(): AuthUser | null {
    try {
      const raw = localStorage.getItem(this.USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch { return null; }
  }
}
