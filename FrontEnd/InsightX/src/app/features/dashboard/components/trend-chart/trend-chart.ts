import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgApexchartsModule, ChartComponent } from 'ng-apexcharts';
import { DashboardTrendDto } from '../../dashboard.service';

export type ChartOptions = {
  series: any;
  chart: any;
  xaxis: any;
  title: any;
  stroke: any;
  dataLabels: any;
  markers: any;
  colors: any;
};

@Component({
  selector: 'app-trend-chart',
  standalone: true,
  imports: [CommonModule, FormsModule, NgApexchartsModule],
  templateUrl: './trend-chart.html',
  styleUrl: './trend-chart.css'
})
export class TrendChart implements OnChanges {
  @ViewChild('chart') chart!: ChartComponent;
  @Input() trends: DashboardTrendDto[] = [];
  @Input() selectedKpiName: string = '';

  public chartOptions: Partial<ChartOptions> | any = {};

  constructor() {
    this.initChart();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['trends'] || changes['selectedKpiName']) {
      this.updateChart();
    }
  }

  onKpiSelectChange(): void {
    this.updateChart();
  }

  selectKpi(kpiName: string): void {
    this.selectedKpiName = kpiName;
    this.updateChart();
  }

  hasHistoryData(): boolean {
    if (!this.trends || this.trends.length === 0) return false;
    const target = this.trends.find(t => t.kpiName === this.selectedKpiName) || this.trends[0];
    if (!target || !target.values || target.values.length === 0) return false;
    return true;
  }

  private initChart() {
    this.chartOptions = {
      series: [],
      chart: {
        height: 340,
        type: 'line',
        zoom: { enabled: false },
        toolbar: { show: false },
        background: 'transparent',
        fontFamily: 'Inter, system-ui, sans-serif'
      },
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth', width: 3 },
      title: { text: 'KPI Trend (Last 6 Months)', align: 'left', style: { color: '#d1d5db', fontSize: '14px', fontWeight: '600' } },
      xaxis: { categories: [], labels: { style: { colors: '#9ca3af', fontSize: '12px' } } },
      yaxis: { labels: { style: { colors: '#9ca3af', fontSize: '12px' } } },
      grid: { borderColor: 'rgba(255, 255, 255, 0.06)', strokeDashArray: 4 },
      tooltip: { theme: 'dark' },
      colors: ['#3b82f6']
    };
  }

  private updateChart() {
    if (!this.trends || this.trends.length === 0) return;
    
    // Select the specified KPI or default to the first one
    let target = this.trends.find(t => t.kpiName === this.selectedKpiName);
    if (!target) {
        target = this.trends[0];
        this.selectedKpiName = target.kpiName;
    }
    if (!target) return;

    this.chartOptions = {
      ...this.chartOptions,
      series: [{
        name: target.kpiName,
        data: target.values
      }],
      xaxis: {
        ...this.chartOptions.xaxis,
        categories: target.labels
      },
      title: {
        ...this.chartOptions.title,
        text: `${target.kpiName} Trend (Last 6 Months)`,
        align: 'left'
      }
    };
  }
}
