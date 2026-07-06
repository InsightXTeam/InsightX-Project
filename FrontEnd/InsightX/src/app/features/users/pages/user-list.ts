import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, UserResponse } from '../../../core/services/user.service';
import { DepartmentService } from '../../../core/services/department.service';
import { ConfirmDialogComponent } from '../../../shared/components/dialog/confirm-dialog';
import { ToastService } from '../../../core/services/toast.service';
import { extractErrorMessage } from '../../../shared/utils/error.utils';
import { AuthService } from '../../../core/services/auth.service';



interface Department {
  id: number;
  name: string;
  managerName?: string | null;
  managerId?: string | null;
}

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmDialogComponent],
  templateUrl: './user-list.html',
  styleUrl: './user-list.css'
})
export class UserListComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);

  // Signals
  readonly users = signal<UserResponse[]>([]);
  readonly currentTab = signal<'all' | 'active' | 'inactive'>('active');
  readonly showDeleteConfirm = signal(false);
  userToDeleteId: string | null = null;
  readonly departments = signal<Department[]>([]);
  readonly isLoading = signal(false);
  readonly showForm = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingUserId = signal<string | null>(null);
  readonly searchQuery = signal('');

  // Computed Check
  readonly isOwner = computed(() => this.authService.currentUser()?.role === 'Owner');
  readonly filteredUsers = computed(() => {
    const tab = this.currentTab();
    const query = this.searchQuery().toLowerCase().trim();
    let filtered = this.users().filter(u => u.role !== 'Owner');

    if (tab === 'active') {
      filtered = filtered.filter(u => !u.isDeleted);
    } else if (tab === 'inactive') {
      filtered = filtered.filter(u => u.isDeleted);
    }

    if (query) {
      filtered = filtered.filter(u => u.name?.toLowerCase().includes(query));
    }

    return filtered;
  });

  // Input states
  newManager = { name: '', email: '', password: '', departmentId: 0 };
  editUserDeptId = 0;

  ngOnInit(): void {
    this.loadUsers();
    if (this.isOwner()) {
      this.loadDepartments();
    }
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.userService.getUsers().subscribe({
      next: (data) => {
        this.users.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to fetch team members.'));
      }
    });
  }

  loadDepartments(): void {
    this.departmentService.getDepartments().subscribe({
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

  setTab(tab: 'all' | 'active' | 'inactive'): void {
    this.currentTab.set(tab);
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

    this.userService.inviteManager(payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.showForm.set(false);
        this.newManager = { name: '', email: '', password: '', departmentId: 0 };
        this.toastService.show('Manager invited successfully');
        this.loadUsers(); // Reload team members
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to invite manager. Make sure the email is not already in use.'));
      }
    });
  }

  triggerDelete(id: string): void {
    this.userToDeleteId = id;
    this.showDeleteConfirm.set(true);
  }

  confirmDelete(): void {
    const id = this.userToDeleteId;
    if (!id) return;

    this.showDeleteConfirm.set(false);
    this.isLoading.set(true);
    this.error.set(null);

    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.toastService.show('Manager deleted successfully');
        this.loadUsers(); // Reload team members
        if (this.isOwner()) this.loadDepartments(); // Reload departments to reflect unassignment
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to delete manager.'));
      }
    });
  }

  readonly showRestoreModal = signal(false);
  userToRestoreId: string | null = null;
  restoreUserDeptId: number = 0;

  triggerRestore(id: string): void {
    this.userToRestoreId = id;
    this.restoreUserDeptId = 0;
    this.showRestoreModal.set(true);
  }

  cancelRestore(): void {
    this.showRestoreModal.set(false);
    this.userToRestoreId = null;
  }

  confirmRestore(): void {
    const id = this.userToRestoreId;
    if (!id || this.restoreUserDeptId === 0) return;

    this.showRestoreModal.set(false);
    this.isLoading.set(true);
    this.error.set(null);

    this.userService.restoreUser(id).subscribe({
      next: () => {
        this.userService.updateUserDepartment(id, this.restoreUserDeptId).subscribe({
          next: () => {
            this.isLoading.set(false);
            this.toastService.show('Manager restored and assigned successfully');
            this.loadUsers(); // Reload team members
            if (this.isOwner()) this.loadDepartments(); // Reload departments to reflect reassignment
          },
          error: (err) => {
            this.isLoading.set(false);
            this.error.set(extractErrorMessage(err, 'Manager was restored but department assignment failed.'));
            this.loadUsers();
          }
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to restore manager.'));
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.userToDeleteId = null;
  }

  startEditUser(user: UserResponse): void {
    this.editingUserId.set(user.id);
    this.editUserDeptId = user.departmentId || 0;
  }

  cancelUserEdit(): void {
    this.editingUserId.set(null);
  }

  saveUserDepartment(user: UserResponse): void {
    this.isLoading.set(true);
    this.error.set(null);

    const payload = {
      departmentId: this.editUserDeptId === 0 ? null : Number(this.editUserDeptId)
    };

    this.userService.updateUserDepartment(user.id, payload.departmentId).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.editingUserId.set(null);
        this.loadUsers(); // Reload team members to see new department
        if (this.isOwner()) this.loadDepartments(); // Reload departments to reflect new manager assignment
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to update manager department.'));
      }
    });
  }

}

