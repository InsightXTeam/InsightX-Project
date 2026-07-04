import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

export interface DepartmentResponse {
  id: number;
  name: string;
  companyId: number;
  managerName?: string | null;
  managerId?: string | null;
}

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './department-list.html',
  styleUrl: './department-list.css'
})
export class DepartmentListComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly apiBase = environment.apiBaseUrl;

  // Signals
  readonly departments = signal<DepartmentResponse[]>([]);
  readonly managers = signal<{ id: string, name: string }[]>([]);
  readonly isLoading = signal(false);
  readonly showForm = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<number | null>(null);

  // Computed Roles
  readonly isOwner = computed(() => this.authService.currentUser()?.role === 'Owner');

  // Input binds
  newDeptName = '';
  editDeptName = '';
  editDeptManagerId = '';

  ngOnInit(): void {
    this.loadDepartments();
    if (this.isOwner()) {
      this.loadManagers();
    }
  }

  loadManagers(): void {
    this.http.get<any[]>(`${this.apiBase}/users`).subscribe({
      next: (users) => {
        const mgrs = (users || [])
          .filter(u => u.role === 'Manager')
          .map(u => ({ id: u.id, name: u.name }));
        this.managers.set(mgrs);
      }
    });
  }

  loadDepartments(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.http.get<DepartmentResponse[]>(`${this.apiBase}/departments`).subscribe({
      next: (data) => {
        this.departments.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to load company departments. Please try again.'));
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.newDeptName = '';
    this.error.set(null);
  }

  saveDepartment(): void {
    const trimmed = this.newDeptName.trim();
    if (!trimmed) return;

    this.isLoading.set(true);
    this.error.set(null);

    this.http.post<DepartmentResponse>(`${this.apiBase}/departments`, { name: trimmed }).subscribe({
      next: (newDept) => {
        // Append newly created department to local array
        this.departments.update(list => [...list, newDept]);
        this.newDeptName = '';
        this.showForm.set(false);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to create department. Please try again.'));
      }
    });
  }

  startEdit(dept: DepartmentResponse): void {
    this.editingId.set(dept.id);
    this.editDeptName = dept.name;
    this.editDeptManagerId = dept.managerId || '';
    this.error.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editDeptName = '';
    this.editDeptManagerId = '';
  }

  updateDepartment(id: number): void {
    const trimmed = this.editDeptName.trim();
    if (!trimmed) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = {
      name: trimmed,
      managerId: this.editDeptManagerId || null
    };

    this.http.put<DepartmentResponse>(`${this.apiBase}/departments/${id}`, payload).subscribe({
      next: (updatedDept) => {
        this.departments.update(list => list.map(d => d.id === id ? updatedDept : d));
        this.loadManagers(); // Reload managers list as their department associations might change
        this.cancelEdit();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to update department. Please try again.'));
      }
    });
  }

  deleteDepartment(id: number): void {
    if (!confirm('Are you sure you want to delete this department? Linked users will be unassigned.')) return;

    this.isLoading.set(true);
    this.error.set(null);

    this.http.delete(`${this.apiBase}/departments/${id}`).subscribe({
      next: () => {
        this.departments.update(list => list.filter(d => d.id !== id));
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to delete department. Please try again.'));
      }
    });
  }

  private extractErrorMessage(err: any, fallback: string): string {
    const body = err?.error;
    if (typeof body === 'string' && body.trim()) return body;
    if (body && typeof body === 'object') {
      return body.error || body.Error || body.message || body.title || fallback;
    }
    if (err?.status === 0) return 'Unable to reach the server. Please check your connection and try again.';
    return fallback;
  }
}
