import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService, DepartmentResponse } from '../../../core/services/department.service';
import { UserService } from '../../../core/services/user.service';
import { ConfirmDialogComponent } from '../../../shared/components/dialog/confirm-dialog';
import { ToastService } from '../../../core/services/toast.service';
import { extractErrorMessage } from '../../../shared/utils/error.utils';


@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmDialogComponent],
  templateUrl: './department-list.html',
  styleUrl: './department-list.css'
})
export class DepartmentListComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly departmentService = inject(DepartmentService);
  private readonly userService = inject(UserService);
  private readonly toastService = inject(ToastService);

  // Signals
  readonly departments = signal<DepartmentResponse[]>([]);
  readonly showDeleteConfirm = signal(false);
  deptToDeleteId: number | null = null;
  readonly managers = signal<{ id: string, name: string, departmentId?: number | null, departmentName?: string | null }[]>([]);
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
    this.userService.getUsers().subscribe({
      next: (users) => {
        const mgrs = (users || [])
          .filter(u => u.role === 'Manager')
          .map(u => ({
            id: u.id,
            name: u.name,
            departmentId: u.departmentId,
            departmentName: u.departmentName
          }));
        this.managers.set(mgrs);
      }
    });
  }

  loadDepartments(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.departmentService.getDepartments().subscribe({
      next: (data) => {
        this.departments.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to load company departments. Please try again.'));
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

    this.departmentService.createDepartment(trimmed).subscribe({
      next: (newDept) => {
        // Append newly created department to local array
        this.departments.update(list => [...list, newDept]);
        this.newDeptName = '';
        this.showForm.set(false);
        this.isLoading.set(false);
        this.toastService.show('Department created successfully');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to create department. Please try again.'));
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

    this.departmentService.updateDepartment(id, payload).subscribe({
      next: (updatedDept) => {
        this.loadDepartments(); // Reload departments to reflect reassignment on other departments
        this.loadManagers(); // Reload managers list as their department associations might change
        this.cancelEdit();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to update department. Please try again.'));
      }
    });
  }

  triggerDelete(id: number): void {
    this.deptToDeleteId = id;
    this.showDeleteConfirm.set(true);
  }

  confirmDelete(): void {
    const id = this.deptToDeleteId;
    if (id === null) return;

    this.showDeleteConfirm.set(false);
    this.isLoading.set(true);
    this.error.set(null);

    this.departmentService.deleteDepartment(id).subscribe({
      next: () => {
        this.departments.update(list => list.filter(d => d.id !== id));
        this.isLoading.set(false);
        this.toastService.show('Department deleted successfully');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to delete department. Please try again.'));
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.deptToDeleteId = null;
  }

}

