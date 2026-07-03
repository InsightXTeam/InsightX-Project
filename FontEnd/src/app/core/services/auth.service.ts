import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserContext {
  id: number;
  username: string;
  email: string;
  role: string;
  companyId: number;
  departmentId: number | null;
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: UserContext;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'insightx_token';
  private readonly userKey = 'insightx_user';
  private currentUserSubject = new BehaviorSubject<UserContext | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private http: HttpClient
  ) {
    this.loadFromStorage();
  }

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  private loadFromStorage(): void {
    if (!this.isBrowser()) return;

    try {
      const storedUser = localStorage.getItem(this.userKey);
      if (storedUser) {
        this.currentUserSubject.next(JSON.parse(storedUser));
      }
    } catch {
      this.logout();
    }
  }

  get currentUserValue(): UserContext | null {
    return this.currentUserSubject.value;
  }

  get accessToken(): string | null {
    if (!this.isBrowser()) return null;
    return localStorage.getItem(this.tokenKey);
  }

  login(email: string, password: string): Observable<boolean> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(
        tap(response => this.persistSession(response)),
        tap(() => true),
        catchError(() => of(null as unknown as LoginResponse))
      ) as unknown as Observable<boolean>;
  }

  loginWithCredentials(email: string, password: string): Observable<LoginResponse | null> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(
        tap(response => this.persistSession(response)),
        catchError(() => of(null))
      );
  }

  private persistSession(response: LoginResponse): void {
    if (!this.isBrowser()) return;

    localStorage.setItem(this.tokenKey, response.accessToken);
    localStorage.setItem(this.userKey, JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
  }

  logout(): void {
    if (this.isBrowser()) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }
    this.currentUserSubject.next(null);
  }

  isAuthenticated(): boolean {
    return !!this.accessToken && !!this.currentUserValue;
  }
}
