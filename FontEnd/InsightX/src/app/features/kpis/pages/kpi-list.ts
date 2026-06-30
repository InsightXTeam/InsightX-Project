import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

export interface KpiResponse {
  id: number;
  name: string;
  threshold: number;
  unit: string;
  companyId: number;
}

@Component({
  selector: 'app-kpi-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './kpi-list.html',
  styleUrl: './kpi-list.css'
})
export class KpiListComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly apiBase = environment.apiBaseUrl;

  // Signals
  readonly kpis = signal<KpiResponse[]>([]);
  readonly isLoading = signal(false);
  readonly showForm = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<number | null>(null);

  // Computed Check
  readonly isOwner = computed(() => this.authService.currentUser()?.role === 'Owner');

  // Input bindings (New KPI)
  newKpiName = '';
  newKpiThreshold: number | null = null;
  newKpiUnit = '%';

  // Input bindings (Edit KPI)
  editKpiName = '';
  editKpiThreshold: number | null = null;
  editKpiUnit = '';

  ngOnInit(): void {
    this.loadKpis();
  }

  loadKpis(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.http.get<KpiResponse[]>(`${this.apiBase}/kpis`).subscribe({
      next: (data) => {
        this.kpis.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to load company KPIs. Please try again.'));
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.newKpiName = '';
    this.newKpiThreshold = null;
    this.newKpiUnit = '%';
    this.error.set(null);
  }

  saveKpi(): void {
    const name = this.newKpiName.trim();
    const threshold = this.newKpiThreshold;
    const unit = this.newKpiUnit.trim();

    if (!name || threshold === null || !unit) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = { name, threshold, unit };

    this.http.post<KpiResponse>(`${this.apiBase}/kpis`, payload).subscribe({
      next: (newKpi) => {
        this.kpis.update(list => [...list, newKpi]);
        this.toggleForm();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to create KPI. Please try again.'));
      }
    });
  }

  startEdit(kpi: KpiResponse): void {
    this.editingId.set(kpi.id);
    this.editKpiName = kpi.name;
    this.editKpiThreshold = kpi.threshold;
    this.editKpiUnit = kpi.unit;
    this.error.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editKpiName = '';
    this.editKpiThreshold = null;
    this.editKpiUnit = '';
  }

  updateKpi(id: number): void {
    const name = this.editKpiName.trim();
    const threshold = this.editKpiThreshold;
    const unit = this.editKpiUnit.trim();

    if (!name || threshold === null || !unit) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = { name, threshold, unit };

    this.http.put<KpiResponse>(`${this.apiBase}/kpis/${id}`, payload).subscribe({
      next: (updatedKpi) => {
        this.kpis.update(list => list.map(k => k.id === id ? updatedKpi : k));
        this.cancelEdit();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to update KPI. Please try again.'));
      }
    });
  }

  deleteKpi(id: number): void {
    if (!confirm('Are you sure you want to delete this KPI?')) return;

    this.isLoading.set(true);
    this.error.set(null);

    this.http.delete(`${this.apiBase}/kpis/${id}`).subscribe({
      next: () => {
        this.kpis.update(list => list.filter(k => k.id !== id));
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(this.extractErrorMessage(err, 'Failed to delete KPI. Please try again.'));
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
