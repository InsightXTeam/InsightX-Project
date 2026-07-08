import { Component, OnInit, inject, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardKpiDto, DashboardTrendDto, DashboardDepartmentPerformanceDto, AlertDto } from '../dashboard.service';
import { AlertsApiService } from '../../alerts/services/alerts-api.service';
import { KpiCards } from '../components/kpi-cards/kpi-cards';
import { TrendChart } from '../components/trend-chart/trend-chart';
import { DepartmentTable } from '../components/department-table/department-table';
import { RecentAlerts } from '../components/recent-alerts/recent-alerts';
import { forkJoin, finalize } from 'rxjs';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, KpiCards, TrendChart, DepartmentTable, RecentAlerts],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.css'
})
export class DashboardPage implements OnInit {
  private dashboardService = inject(DashboardService);

  kpis: DashboardKpiDto[] = [];
  trends: DashboardTrendDto[] = [];
  departments: DashboardDepartmentPerformanceDto[] = [];
  alerts: AlertDto[] = [];

  isLoading = true;
  isTrendsLoading = false;
  selectedMonths = 6;
  errorMessage = '';
  private loadedCount = 0;
  private totalEndpoints = 4;
  private cdr = inject(ChangeDetectorRef);
  private alertsService = inject(AlertsApiService);

  hasCriticalAlerts(): boolean {
    return this.kpis.some(k => k.status === 'Critical') || this.alerts.some(a => !a.seenByOwner && a.alertType === 2);
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  private checkLoadingComplete() {
    this.loadedCount++;
    if (this.loadedCount >= this.totalEndpoints) {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  onMonthsChange(months: number): void {
    if (this.selectedMonths === months) return;
    this.selectedMonths = months;
    this.isTrendsLoading = true;
    this.dashboardService.getTrends(this.selectedMonths).pipe(
      finalize(() => {
        this.isTrendsLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res) => {
        this.trends = res;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  loadDashboardData() {
    this.isLoading = true;
    this.errorMessage = '';
    this.loadedCount = 0;
    
    this.dashboardService.getKpisSummary().subscribe({
      next: (res) => { this.kpis = res; this.checkLoadingComplete(); },
      error: (err) => { console.error(err); this.errorMessage = 'Failed to load KPIs'; this.checkLoadingComplete(); }
    });

    this.isTrendsLoading = true;
    this.dashboardService.getTrends(this.selectedMonths).pipe(
      finalize(() => {
        this.isTrendsLoading = false;
        this.checkLoadingComplete();
      })
    ).subscribe({
      next: (res) => { this.trends = res; },
      error: (err) => { console.error(err); this.errorMessage = 'Failed to load Trends'; }
    });

    this.dashboardService.getDepartmentsPerformance().subscribe({
      next: (res) => { this.departments = res; this.checkLoadingComplete(); },
      error: (err) => { console.error(err); this.errorMessage = 'Failed to load Departments'; this.checkLoadingComplete(); }
    });

    this.dashboardService.getRecentAlerts().subscribe({
      next: (res) => { this.alerts = res; this.checkLoadingComplete(); },
      error: (err) => { console.error(err); this.errorMessage = 'Failed to load Alerts'; this.checkLoadingComplete(); }
    });
  }

  onDeleteAlert(alertId: number): void {
    this.alertsService.deleteAlert(alertId).subscribe({
      next: () => {
        this.alerts = this.alerts.filter(a => a.id !== alertId);
        this.alertsService.fetchUnseenCount();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to delete alert', err);
      }
    });
  }
}
