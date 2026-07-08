import { Component, OnInit, inject, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardKpiDto, DashboardTrendDto, DashboardDepartmentPerformanceDto, AlertDto } from '../dashboard.service';
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
  errorMessage = '';
  private loadedCount = 0;
  private totalEndpoints = 4;
  private cdr = inject(ChangeDetectorRef);

  isDropdownOpen = false;

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.bell-icon-wrapper')) {
      this.isDropdownOpen = false;
    }
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

  loadDashboardData() {
    this.isLoading = true;
    this.errorMessage = '';
    this.loadedCount = 0;
    
    this.dashboardService.getKpisSummary().subscribe({
      next: (res) => { this.kpis = res; this.checkLoadingComplete(); },
      error: (err) => { console.error(err); this.errorMessage = 'Failed to load KPIs'; this.checkLoadingComplete(); }
    });

    this.dashboardService.getTrends().subscribe({
      next: (res) => { this.trends = res; this.checkLoadingComplete(); },
      error: (err) => { console.error(err); this.errorMessage = 'Failed to load Trends'; this.checkLoadingComplete(); }
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
}
