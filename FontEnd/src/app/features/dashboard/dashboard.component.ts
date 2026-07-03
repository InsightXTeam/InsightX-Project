import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService, UserContext } from '../../core/services/auth.service';
import {
  DashboardAlert,
  DepartmentSummary,
  KpiCard,
  TrendPoint
} from '../../core/models/dashboard.models';

declare var ApexCharts: any;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chartContainer') chartContainer?: ElementRef<HTMLDivElement>;

  public kpis: KpiCard[] = [];
  public departments: DepartmentSummary[] = [];
  public alerts: DashboardAlert[] = [];
  public currentUser: UserContext | null = null;
  public selectedKpi = 'Production';
  public isLoading = true;
  public hasChartData = false;

  public readonly kpiOptions = ['Production', 'Defect Rate', 'Absent Employees', 'Revenue'];

  private chartInstance: any = null;
  private trendPoints: TrendPoint[] = [];

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => (this.currentUser = user));
    this.loadDashboard();
  }

  ngAfterViewInit(): void {
    if (this.trendPoints.length > 0) {
      this.renderChart();
    }
  }

  ngOnDestroy(): void {
    this.chartInstance?.destroy();
  }

  loadDashboard(): void {
    this.isLoading = true;

    this.dashboardService.getKpis().subscribe(kpis => {
      this.kpis = kpis;
      if (kpis.length > 0 && !this.kpiOptions.includes(this.selectedKpi)) {
        this.selectedKpi = kpis[0].kpiName;
      }
      this.loadTrends();
    });

    this.dashboardService.getDepartments().subscribe(departments => {
      this.departments = departments;
    });

    this.dashboardService.getRecentAlerts().subscribe(alerts => {
      this.alerts = alerts;
      this.isLoading = false;
    });
  }

  onKpiChange(kpiName: string): void {
    this.selectedKpi = kpiName;
    this.loadTrends();
  }

  loadTrends(): void {
    this.dashboardService.getTrends(this.selectedKpi).subscribe(points => {
      this.trendPoints = points;
      this.hasChartData = points.length > 0;
      setTimeout(() => this.renderChart());
    });
  }

  renderChart(): void {
    if (!this.chartContainer?.nativeElement || this.trendPoints.length === 0) {
      return;
    }

    this.chartInstance?.destroy();

    const options = {
      series: [{
        name: this.selectedKpi,
        data: this.trendPoints.map(p => p.value)
      }],
      chart: {
        type: 'line',
        height: 320,
        background: 'transparent',
        toolbar: { show: false },
        fontFamily: 'Inter, sans-serif'
      },
      colors: ['#6366f1'],
      stroke: { curve: 'smooth', width: 3 },
      markers: { size: 5, strokeWidth: 2 },
      dataLabels: { enabled: false },
      grid: {
        borderColor: 'rgba(255,255,255,0.06)',
        strokeDashArray: 4
      },
      xaxis: {
        categories: this.trendPoints.map(p => `${p.month} ${p.year}`),
        labels: { style: { colors: '#94a3b8' } },
        axisBorder: { show: false },
        axisTicks: { show: false }
      },
      yaxis: {
        labels: { style: { colors: '#94a3b8' } }
      },
      tooltip: {
        theme: 'dark',
        y: {
          formatter: (val: number) => this.formatValue(val, this.selectedKpi)
        }
      }
    };

    this.chartInstance = new ApexCharts(this.chartContainer.nativeElement, options);
    this.chartInstance.render();
  }

  formatValue(value: number, kpiName: string): string {
    const kpi = this.kpis.find(k => k.kpiName === kpiName);
    if (kpi?.isPercentage) return `${value}%`;
    if (kpiName === 'Revenue') return `$${value.toLocaleString()}`;
    return value.toLocaleString();
  }

  getKpiIcon(kpiName: string): string {
    switch (kpiName) {
      case 'Production': return 'bi-box-seam-fill';
      case 'Defect Rate': return 'bi-patch-exclamation-fill';
      case 'Absent Employees': return 'bi-people-fill';
      case 'Revenue': return 'bi-currency-dollar';
      default: return 'bi-graph-up';
    }
  }

  getDashboardTitle(): string {
    if (this.currentUser?.role === 'Owner') {
      return 'Company Overview';
    }
    return `${this.currentUser?.username ?? 'Department'} Dashboard`;
  }
}
