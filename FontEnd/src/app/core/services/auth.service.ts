import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';

export interface UserContext {
  id: number;
  username: string;
  email: string;
  role: string; // 'Owner' | 'Manager'
  companyId: number;
  departmentId: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<UserContext | null>(null);
  public currentUser$: Observable<UserContext | null> = this.currentUserSubject.asObservable();

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.loadUserFromStorage();
  }

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  private loadUserFromStorage(): void {
    if (!this.isBrowser()) return;
    try {
      const stored = localStorage.getItem('insightx_user');
      if (stored) {
        this.currentUserSubject.next(JSON.parse(stored));
      }
    } catch (e) {
      console.error('Error reading auth state from localStorage', e);
    }
  }

  public get currentUserValue(): UserContext | null {
    return this.currentUserSubject.value;
  }

  public login(role: string, departmentId: number | null): void {
    let user: UserContext;

    if (role === 'Owner') {
      user = {
        id: 1,
        username: 'Owner',
        email: 'owner@insightx.com',
        role: 'Owner',
        companyId: 1,
        departmentId: null
      };
    } else {
      // Manager
      const deptName = departmentId === 1 ? 'Production' : departmentId === 2 ? 'Quality Control' : 'HR';
      user = {
        id: departmentId === 1 ? 2 : departmentId === 2 ? 3 : 4,
        username: `${deptName} Manager`,
        email: `${deptName.toLowerCase().replace(' ', '')}_mgr@insightx.com`,
        role: 'Manager',
        companyId: 1,
        departmentId: departmentId
      };
    }

    if (this.isBrowser()) {
      localStorage.setItem('insightx_user', JSON.stringify(user));
    }
    this.currentUserSubject.next(user);
  }

  public logout(): void {
    if (this.isBrowser()) {
      localStorage.removeItem('insightx_user');
    }
    this.currentUserSubject.next(null);
  }

  public isAuthenticated(): boolean {
    return this.currentUserSubject.value !== null;
  }
}
