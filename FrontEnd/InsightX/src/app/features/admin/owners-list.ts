import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, OwnerResponse } from '../../core/services/admin.service';
import { extractErrorMessage } from '../../shared/utils/error.utils';



@Component({
  selector: 'app-owners-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './owners-list.html',
  styleUrl: './owners-list.css'
})
export class OwnersListComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  readonly owners = signal<OwnerResponse[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadOwners();
  }

  loadOwners(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.adminService.getOwners().subscribe({
      next: (data) => {
        this.owners.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to fetch platform company owners.'));
      }
    });
  }

  toggleActivation(owner: OwnerResponse): void {
    this.isLoading.set(true);
    const isActivate = !owner.isActivated;
    const action = isActivate ? 'activate' : 'deactivate';

    const request = isActivate
      ? this.adminService.activateOwner(owner.userId)
      : this.adminService.deactivateOwner(owner.userId);

    request.subscribe({
      next: () => {
        // Toggle the state in local list
        this.owners.update(list =>
          list.map(o => o.userId === owner.userId ? { ...o, isActivated: !o.isActivated } : o)
        );
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, `Failed to ${action} owner account.`));
      }
    });
  }

}

