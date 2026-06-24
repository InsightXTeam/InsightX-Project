import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

interface UserResponse {
  id: string;
  name: string;
  email: string;
  role: string;
  departmentId: number | null;
  departmentName: string | null;
}

interface Department {
  id: number;
  name: string;
}

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Team Members</h1>
          <p class="page-subtitle">Invite managers and view active team members inside your organization.</p>
        </div>
        @if (isOwner()) {
          <button class="add-user-btn" (click)="toggleForm()" type="button">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" style="width: 18px; height: 18px;">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 7.5v3m0 0v3m0-3h3m-3 0h-3m-2.25-4.125a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zM4 19.235v-.11a6.375 6.375 0 0112.75 0v.109A12.318 12.318 0 0110.374 21c-2.331 0-4.512-.645-6.374-1.766z" />
            </svg>
            Invite Manager
          </button>
        }
      </div>

      <!-- Alerts -->
      @if (error()) {
        <div class="alert-error" role="alert">
          {{ error() }}
        </div>
      }

      <!-- Invite Form (Owner Only) -->
      @if (showForm() && isOwner()) {
        <div class="invite-form-panel">
          <h3 class="form-title">Invite New Manager</h3>
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Full Name</label>
              <input type="text" [(ngModel)]="newManager.name" placeholder="Alice Smith" class="input-field" />
            </div>
            <div class="form-group">
              <label class="form-label">Email Address</label>
              <input type="email" [(ngModel)]="newManager.email" placeholder="alice@acme.com" class="input-field" />
            </div>
            <div class="form-group">
              <label class="form-label">Temporary Password</label>
              <input type="password" [(ngModel)]="newManager.password" placeholder="At least 8 chars, 1 digit" class="input-field" />
            </div>
            <div class="form-group">
              <label class="form-label">Assign Department</label>
              <select [(ngModel)]="newManager.departmentId" class="input-field select-field">
                <option [value]="0" disabled selected>Select Department</option>
                @for (dept of departments(); track dept.id) {
                  <option [value]="dept.id">{{ dept.name }}</option>
                }
              </select>
            </div>
          </div>
          <div class="form-actions">
            <button class="cancel-btn" (click)="toggleForm()" [disabled]="isLoading()" type="button">Cancel</button>
            <button class="save-btn" (click)="submitInvitation()" [disabled]="!newManager.name || !newManager.email || !newManager.password || !newManager.departmentId || isLoading()" type="button">
              {{ isLoading() ? 'Sending Invite...' : 'Send Invitation' }}
            </button>
          </div>
        </div>
      }

      <!-- Users Table Grid -->
      <div class="table-container">
        <table class="users-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Department</th>
              @if (isOwner()) {
                <th>Actions</th>
              }
            </tr>
          </thead>
          <tbody>
            @for (user of users(); track user.id) {
              <tr>
                <td style="font-weight: 600;">{{ user.name }}</td>
                <td style="color: #64748b;">{{ user.email }}</td>
                <td>
                  <span class="role-badge" [class]="user.role.toLowerCase()">
                    {{ user.role }}
                  </span>
                </td>
                <td>
                  <span class="dept-text">
                    {{ user.departmentName || 'All Departments' }}
                  </span>
                </td>
                @if (isOwner()) {
                  <td>
                    @if (user.role === 'Manager') {
                      <button class="delete-btn" (click)="deleteUser(user.id)" type="button" title="Delete Manager">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" style="width: 16px; height: 16px;">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                        </svg>
                      </button>
                    }
                  </td>
                }
              </tr>
            } @empty {
              <tr>
                <td [attr.colspan]="isOwner() ? 5 : 4" style="text-align: center; color: #94a3b8; padding: 40px;">No team members found.</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .page-container {
      padding: 30px;
      font-family: 'Inter', sans-serif;
      background-color: var(--bg-primary);
      min-height: 100vh;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
    }
    .page-title {
      font-size: 24px;
      font-weight: 700;
      color: white;
      margin: 0 0 6px 0;
    }
    .page-subtitle {
      font-size: 14px;
      color: var(--text-secondary);
      margin: 0;
    }
    .add-user-btn {
      background: var(--btn-primary);
      color: white;
      border: none;
      padding: 10px 18px;
      border-radius: 8px;
      font-weight: 600;
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
      transition: all 0.2s ease;
    }
    .add-user-btn:hover {
      background: var(--btn-hover);
      transform: translateY(-1px);
    }
    
    /* Invite Panel */
    .invite-form-panel {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 24px;
      margin-bottom: 30px;
      box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
      animation: slideDown 0.3s cubic-bezier(0.16, 1, 0.3, 1);
    }
    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .form-title {
      font-size: 16px;
      font-weight: 600;
      color: white;
      margin: 0 0 20px 0;
    }
    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 16px;
      margin-bottom: 20px;
    }
    .form-group { display: flex; flex-direction: column; gap: 6px; }
    .form-label { font-size: 11px; font-weight: 600; color: var(--text-secondary); text-transform: uppercase; }
    .input-field {
      padding: 10px 12px;
      background: rgba(15, 23, 42, 0.6);
      border: 1px solid var(--border-color);
      color: white;
      border-radius: 8px;
      font-size: 14px;
      outline: none;
      transition: all 0.2s ease;
    }
    .input-field:focus { border-color: var(--btn-primary); box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15); }
    .select-field { appearance: none; background-image: url('data:image/svg+xml;charset=utf-8,%3Csvg xmlns=\'http:%2F%2Fwww.w3.org/2000/svg\' fill=\'none\' viewBox=\'0 0 20 20\'%3E%3Cpath stroke=\'%236b7280\' stroke-linecap=\'round\' stroke-linejoin=\'round\' stroke-width=\'1.5\' d=\'m6 8 4 4 4-4\'/%3E%3C/svg%3E'); background-position: right 10px center; background-repeat: no-repeat; background-size: 20px; }
    
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; }
    .cancel-btn { background: transparent; border: 1px solid var(--border-color); color: var(--text-secondary); padding: 10px 16px; border-radius: 8px; font-weight: 600; font-size: 14px; cursor: pointer; }
    .cancel-btn:hover { background: rgba(255, 255, 255, 0.05); color: white; }
    .save-btn { background: var(--btn-primary); color: white; border: none; padding: 10px 20px; border-radius: 8px; font-weight: 600; font-size: 14px; cursor: pointer; }
    .save-btn:hover:not(:disabled) { background: var(--btn-hover); }
    
    /* Users Table */
    .table-container {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
    }
    .users-table { width: 100%; border-collapse: collapse; text-align: left; font-size: 14px; }
    .users-table th { background: rgba(255, 255, 255, 0.02); padding: 16px; font-weight: 600; color: var(--text-primary); border-bottom: 1px solid var(--border-color); }
    .users-table td { padding: 16px; border-bottom: 1px solid var(--border-color); color: var(--text-primary); }
    .users-table tr:last-child td { border-bottom: none; }
    
    .role-badge { font-size: 11px; font-weight: 600; padding: 4px 8px; border-radius: 12px; display: inline-block; text-transform: capitalize; }
    .role-badge.owner { background: rgba(79, 70, 229, 0.2); color: #818cf8; }
    .role-badge.manager { background: rgba(59, 130, 246, 0.2); color: #60a5fa; }
    .role-badge.sadmin { background: rgba(147, 51, 234, 0.2); color: #c084fc; }
    .dept-text { color: var(--text-secondary); font-weight: 500; }
    .alert-error { background: rgba(239, 68, 68, 0.08); border: 1px solid rgba(239, 68, 68, 0.2); color: #fca5a5; padding: 12px 16px; border-radius: 8px; font-size: 14px; margin-bottom: 20px; }
    .delete-btn {
      background: transparent;
      border: none;
      color: #ef4444;
      cursor: pointer;
      padding: 6px;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s ease;
    }
    .delete-btn:hover {
      background: rgba(239, 68, 68, 0.1);
      color: #f87171;
    }
  `]
})
export class UserListComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly apiBase = environment.apiBaseUrl;

  // Signals
  readonly users = signal<UserResponse[]>([]);
  readonly departments = signal<Department[]>([]);
  readonly isLoading = signal(false);
  readonly showForm = signal(false);
  readonly error = signal<string | null>(null);

  // Computed Check
  readonly isOwner = computed(() => this.authService.currentUser()?.role === 'Owner');

  // Input states
  newManager = { name: '', email: '', password: '', departmentId: 0 };

  ngOnInit(): void {
    this.loadUsers();
    if (this.isOwner()) {
      this.loadDepartments();
    }
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.http.get<UserResponse[]>(`${this.apiBase}/users`).subscribe({
      next: (data) => {
        this.users.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error || 'Failed to fetch team members.');
      }
    });
  }

  loadDepartments(): void {
    this.http.get<Department[]>(`${this.apiBase}/departments`).subscribe({
      next: (data) => {
        this.departments.set(data || []);
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.newManager = { name: '', email: '', password: '', departmentId: 0 };
    this.error.set(null);
  }

  submitInvitation(): void {
    const { name, email, password, departmentId } = this.newManager;
    if (!name || !email || !password || !departmentId) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = {
      name,
      email,
      password,
      departmentId: Number(departmentId)
    };

    this.http.post(`${this.apiBase}/users/invite`, payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.showForm.set(false);
        this.newManager = { name: '', email: '', password: '', departmentId: 0 };
        this.loadUsers(); // Reload team members
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error || 'Failed to invite manager. Make sure email is not in use.');
      }
    });
  }

  deleteUser(id: string): void {
    if (!confirm('Are you sure you want to delete this manager?')) return;

    this.isLoading.set(true);
    this.error.set(null);

    this.http.delete(`${this.apiBase}/users/${id}`).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.loadUsers(); // Reload team members
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error || 'Failed to delete manager.');
      }
    });
  }
}
