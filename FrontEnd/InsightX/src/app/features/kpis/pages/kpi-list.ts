import { Component, OnInit, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { KpiService, KpiResponse } from '../../../core/services/kpi.service';
import { DepartmentService, DepartmentResponse } from '../../../core/services/department.service';
import { ConfirmDialogComponent } from '../../../shared/components/dialog/confirm-dialog';
import { ToastService } from '../../../core/services/toast.service';
import { extractErrorMessage } from '../../../shared/utils/error.utils';
import { AuthService } from '../../../core/services/auth.service';
import { HelpTooltipComponent } from '../../../shared/components/help-tooltip/help-tooltip.component';



@Component({
  selector: 'app-kpi-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmDialogComponent, HelpTooltipComponent],
  templateUrl: './kpi-list.html',
  styleUrl: './kpi-list.css'
})
export class KpiListComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly kpiService = inject(KpiService);
  private readonly departmentService = inject(DepartmentService);
  private readonly toastService = inject(ToastService);

  // Signals
  readonly kpis = signal<KpiResponse[]>([]);
  readonly departments = signal<DepartmentResponse[]>([]);
  readonly selectedFilterDepartmentId = signal<number | null | 'ALL' | string>('ALL');
  readonly showDeleteConfirm = signal(false);
  kpiToDeleteId: number | null = null;
  readonly isLoading = signal(false);
  readonly showForm = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<number | null>(null);
  readonly isFilterDropdownOpen = signal(false);

  // Computed Check
  readonly isOwner = computed(() => this.authService.currentUser()?.role === 'Owner');

  readonly filteredKpis = computed(() => {
    const filter = this.selectedFilterDepartmentId();
    const list = this.kpis();
    if (filter === 'ALL') return list;
    if (filter === null || String(filter) === 'null') return list.filter(k => k.departmentId === null || k.departmentId === undefined);
    return list.filter(k => k.departmentId === Number(filter));
  });

  // Input bindings (New KPI)
  newKpiName = '';
  newKpiThreshold: number | null = null;
  newKpiUnit = '%';
  newKpiAlertPercentageDiff: number | null = null;
  newKpiTrendMonthsCount: number | null = null;
  newKpiThresholdDirection = 1;
  newKpiDepartmentId: number | null = null;
  newKpiDescription = '';

  // Input bindings (Edit KPI)
  editKpiName = '';
  editKpiThreshold: number | null = null;
  editKpiUnit = '';
  editKpiAlertPercentageDiff: number | null = null;
  editKpiTrendMonthsCount: number | null = null;
  editKpiThresholdDirection = 1;
  editKpiDepartmentId: number | null = null;
  editKpiDescription = '';

  ngOnInit(): void {
    this.loadKpis();
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.departmentService.getDepartments().subscribe({
      next: (data) => this.departments.set(data || []),
      error: (err) => console.error('Failed to load departments', err)
    });
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
    this.newKpiDepartmentId = null;
    this.newKpiDescription = '';
    this.error.set(null);
  }

  saveKpi(): void {
    const name = this.newKpiName.trim();
    const threshold = this.newKpiThreshold;
    const unit = this.newKpiUnit.trim();
    const alertPercentageDiff = this.newKpiAlertPercentageDiff;
    const trendMonthsCount = this.newKpiTrendMonthsCount;
    const thresholdDirection = Number(this.newKpiThresholdDirection);
    const departmentId = (this.newKpiDepartmentId === null || this.newKpiDepartmentId === undefined || String(this.newKpiDepartmentId) === 'null' || String(this.newKpiDepartmentId) === '') ? null : Number(this.newKpiDepartmentId);
    const description = this.newKpiDescription.trim() || null;

    if (!name || threshold === null || !unit || alertPercentageDiff === null || trendMonthsCount === null) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = { name, threshold, unit, alertPercentageDiff, trendMonthsCount, thresholdDirection, departmentId, description };

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
    this.editKpiDepartmentId = kpi.departmentId ?? null;
    this.editKpiDescription = kpi.description ?? '';
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
    this.editKpiDepartmentId = null;
    this.editKpiDescription = '';
  }

  updateKpi(id?: number | null): void {
    const targetId = id ?? this.editingId();
    if (targetId === null) return;

    const name = this.editKpiName.trim();
    const threshold = this.editKpiThreshold;
    const unit = this.editKpiUnit.trim();
    const alertPercentageDiff = this.editKpiAlertPercentageDiff;
    const trendMonthsCount = this.editKpiTrendMonthsCount;
    const thresholdDirection = Number(this.editKpiThresholdDirection);
    const departmentId = (this.editKpiDepartmentId === null || this.editKpiDepartmentId === undefined || String(this.editKpiDepartmentId) === 'null' || String(this.editKpiDepartmentId) === '') ? null : Number(this.editKpiDepartmentId);
    const description = this.editKpiDescription.trim() || null;

    if (!name || threshold === null || !unit || alertPercentageDiff === null || trendMonthsCount === null) return;

    this.isLoading.set(true);
    this.error.set(null);

    const payload = { name, threshold, unit, alertPercentageDiff, trendMonthsCount, thresholdDirection, departmentId, description };

    this.kpiService.updateKpi(targetId, payload).subscribe({
      next: (updatedKpi) => {
        this.kpis.update(list => list.map(k => k.id === targetId ? updatedKpi : k));
        this.cancelEdit();
        this.isLoading.set(false);
        this.toastService.show('KPI updated successfully');
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

  getDepartmentName(deptId?: number | null, deptName?: string | null): string {
    if (deptName) return deptName;
    if (!deptId) return 'Company-Wide';
    const dept = this.departments().find(d => d.id === deptId);
    return dept ? dept.name : 'Company-Wide';
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target?.closest('.custom-filter-dropdown')) {
      this.isFilterDropdownOpen.set(false);
    }
  }

  toggleFilterDropdown(): void {
    this.isFilterDropdownOpen.update(val => !val);
  }

  selectFilterDepartment(val: number | null | 'ALL' | string): void {
    const targetVal = val === 'null' ? null : (val as number | null | 'ALL');
    this.selectedFilterDepartmentId.set(targetVal);
    this.isFilterDropdownOpen.set(false);
  }

  getSelectedFilterLabel(): string {
    const filter = this.selectedFilterDepartmentId();
    if (filter === 'ALL') return 'All KPIs (Show Everything)';
    if (filter === null || String(filter) === 'null') return 'Company-Wide (All Departments)';
    const dept = this.departments().find(d => d.id === Number(filter));
    return dept ? dept.name : 'Company-Wide (All Departments)';
  }

}

