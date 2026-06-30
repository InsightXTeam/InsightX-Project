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
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Platform Owners</h1>
          <p class="page-subtitle">Super Admin control panel to manage company owner registrations and system access.</p>
        </div>
      </div>

      <!-- Error Alerts -->
      @if (error()) {
        <div class="alert-error" role="alert">
          {{ error() }}
        </div>
      }

      <!-- Owners Grid Table -->
      <div class="table-container">
        <table class="owners-table">
          <thead>
            <tr>
              <th>Owner Name</th>
              <th>Email</th>
              <th>Company</th>
              <th>Registered At</th>
              <th>Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            @for (owner of owners(); track owner.userId) {
              <tr>
                <td style="font-weight: 600;">{{ owner.ownerName }}</td>
                <td style="color: #64748b;">{{ owner.ownerEmail }}</td>
                <td style="font-weight: 500; color: #4f46e5;">{{ owner.companyName }} (ID: {{ owner.companyId }})</td>
                <td>{{ owner.companyCreatedAt | date:'shortDate' }}</td>
                <td>
                  <span class="status-badge" [class.active]="owner.isActivated" [class.inactive]="!owner.isActivated">
                    {{ owner.isActivated ? 'Activated' : 'Deactivated' }}
                  </span>
                </td>
                <td>
                  @if (owner.isActivated) {
                    <button class="action-btn deactivate" (click)="toggleActivation(owner)" [disabled]="isLoading()">
                      Deactivate
                    </button>
                  } @else {
                    <button class="action-btn activate" (click)="toggleActivation(owner)" [disabled]="isLoading()">
                      Activate
                    </button>
                  }
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6" style="text-align: center; color: #94a3b8; padding: 40px;">No platform owners registered.</td>
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
    
    /* Table styling */
    .table-container {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
    }
    .owners-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
      font-size: 14px;
    }
    .owners-table th {
      background: rgba(255, 255, 255, 0.02);
      padding: 16px;
      font-weight: 600;
      color: var(--text-primary);
      border-bottom: 1px solid var(--border-color);
    }
    .owners-table td {
      padding: 16px;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
    }
    .owners-table tr:last-child td {
      border-bottom: none;
    }
    
    /* Badge and buttons */
    .status-badge {
      font-size: 11px;
      font-weight: 600;
      padding: 4px 8px;
      border-radius: 12px;
      display: inline-block;
    }
    .status-badge.active { background: rgba(16, 185, 129, 0.2); color: #10b981; }
    .status-badge.inactive { background: rgba(239, 68, 68, 0.2); color: #ef4444; }
    
    .action-btn {
      border: none;
      padding: 6px 12px;
      border-radius: 6px;
      font-weight: 600;
      font-size: 13px;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .action-btn.activate { background: rgba(16, 185, 129, 0.2); color: #10b981; }
    .action-btn.activate:hover { background: #10b981; color: white; }
    .action-btn.deactivate { background: rgba(239, 68, 68, 0.2); color: #ef4444; }
    .action-btn.deactivate:hover { background: #ef4444; color: white; }
    
    .alert-error {
      background: rgba(239, 68, 68, 0.08);
      border: 1px solid rgba(239, 68, 68, 0.2);
      color: #fca5a5;
      padding: 12px 16px;
      border-radius: 8px;
      font-size: 14px;
      margin-bottom: 20px;
    }
  `]
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
