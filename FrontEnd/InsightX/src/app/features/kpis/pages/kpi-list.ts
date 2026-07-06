import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { KpiService, KpiResponse } from '../../../core/services/kpi.service';
import { ConfirmDialogComponent } from '../../../shared/components/dialog/confirm-dialog';
import { ToastService } from '../../../core/services/toast.service';
import { extractErrorMessage } from '../../../shared/utils/error.utils';
import { AuthService } from '../../../core/services/auth.service';



@Component({
  selector: 'app-kpi-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmDialogComponent],
  templateUrl: './kpi-list.html',
  styleUrl: './kpi-list.css'
})
export class KpiListComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly kpiService = inject(KpiService);
  private readonly toastService = inject(ToastService);

  // Signals
  readonly kpis = signal<KpiResponse[]>([]);
  readonly showDeleteConfirm = signal(false);
  kpiToDeleteId: number | null = null;
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
  newKpiAlertPercentageDiff: number | null = null;
  newKpiTrendMonthsCount: number | null = null;
  newKpiThresholdDirection = 1;

  // Input bindings (Edit KPI)
  editKpiName = '';
  editKpiThreshold: number | null = null;
  editKpiUnit = '';
  editKpiAlertPercentageDiff: number | null = null;
  editKpiTrendMonthsCount: number | null = null;
  editKpiThresholdDirection = 1;

  ngOnInit(): void {
    this.loadKpis();
  }

  loadKpis(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.kpiService.getKpis().subscribe({
      next: (data) => {
        this.kpis.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to load company KPIs. Please try again.'));
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(val => !val);
    this.newKpiName = '';
    this.newKpiThreshold = null;
    this.newKpiUnit = '%';
    this.newKpiAlertPercentageDiff = null;
    this.newKpiTrendMonthsCount = null;
    this.newKpiThresholdDirection = 1;
    this.error.set(null);
  }

  saveKpi(): void {
    const name = this.newKpiName.trim();
    const threshold = this.newKpiThreshold;
    const unit = this.newKpiUnit.trim();
    const alertPercentageDiff = this.newKpiAlertPercentageDiff;
    const trendMonthsCount = this.newKpiTrendMonthsCount;
    const thresholdDirection = Number(this.newKpiThresholdDirection);

    if (!name || threshold === null || !unit || alertPercentageDiff === null || trendMonthsCount === null) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = { name, threshold, unit, alertPercentageDiff, trendMonthsCount, thresholdDirection };

    this.kpiService.createKpi(payload).subscribe({
      next: (newKpi) => {
        this.kpis.update(list => [...list, newKpi]);
        this.toggleForm();
        this.isLoading.set(false);
        this.toastService.show('KPI created successfully');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to create KPI. Please try again.'));
      }
    });
  }

  startEdit(kpi: KpiResponse): void {
    this.editingId.set(kpi.id);
    this.editKpiName = kpi.name;
    this.editKpiThreshold = kpi.threshold;
    this.editKpiUnit = kpi.unit;
    this.editKpiAlertPercentageDiff = kpi.alertPercentageDiff;
    this.editKpiTrendMonthsCount = kpi.trendMonthsCount;
    this.editKpiThresholdDirection = kpi.thresholdDirection ?? 1;
    this.error.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editKpiName = '';
    this.editKpiThreshold = null;
    this.editKpiUnit = '';
    this.editKpiAlertPercentageDiff = null;
    this.editKpiTrendMonthsCount = null;
    this.editKpiThresholdDirection = 1;
  }

  updateKpi(id: number): void {
    const name = this.editKpiName.trim();
    const threshold = this.editKpiThreshold;
    const unit = this.editKpiUnit.trim();
    const alertPercentageDiff = this.editKpiAlertPercentageDiff;
    const trendMonthsCount = this.editKpiTrendMonthsCount;
    const thresholdDirection = Number(this.editKpiThresholdDirection);

    if (!name || threshold === null || !unit || alertPercentageDiff === null || trendMonthsCount === null) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = { name, threshold, unit, alertPercentageDiff, trendMonthsCount, thresholdDirection };

    this.kpiService.updateKpi(id, payload).subscribe({
      next: (updatedKpi) => {
        this.kpis.update(list => list.map(k => k.id === id ? updatedKpi : k));
        this.cancelEdit();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to update KPI. Please try again.'));
      }
    });
  }

  triggerDelete(id: number): void {
    this.kpiToDeleteId = id;
    this.showDeleteConfirm.set(true);
  }

  confirmDelete(): void {
    const id = this.kpiToDeleteId;
    if (id === null) return;

    this.showDeleteConfirm.set(false);
    this.isLoading.set(true);
    this.error.set(null);

    this.kpiService.deleteKpi(id).subscribe({
      next: () => {
        this.kpis.update(list => list.filter(k => k.id !== id));
        this.isLoading.set(false);
        this.toastService.show('KPI deleted successfully');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(extractErrorMessage(err, 'Failed to delete KPI. Please try again.'));
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.kpiToDeleteId = null;
  }

}

