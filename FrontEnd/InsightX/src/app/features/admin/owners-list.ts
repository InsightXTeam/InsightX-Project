import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface OwnerResponse {
  userId: string;
  ownerName: string;
  ownerEmail: string;
  isActivated: boolean;
  companyId: number;
  companyName: string;
  companyCreatedAt: string;
}

@Component({
  selector: 'app-owners-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './owners-list.html',
  styleUrl: './owners-list.css'
})
export class OwnersListComponent implements OnInit {
  private readonly http = inject(HttpClient);

  readonly owners = signal<OwnerResponse[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadOwners();
  }

  loadOwners(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.http.get<OwnerResponse[]>(`${environment.apiBaseUrl}/users/owners`).subscribe({
      next: (data) => {
        this.owners.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to fetch platform company owners.'));
      }
    });
  }

  toggleActivation(owner: OwnerResponse): void {
    this.isLoading.set(true);
    const action = owner.isActivated ? 'deactivate' : 'activate';
    
    this.http.post(`${environment.apiBaseUrl}/users/${owner.userId}/${action}`, {}).subscribe({
      next: () => {
        // Toggle the state in local list
        this.owners.update(list => 
          list.map(o => o.userId === owner.userId ? { ...o, isActivated: !o.isActivated } : o)
        );
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, `Failed to ${action} owner account.`));
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
