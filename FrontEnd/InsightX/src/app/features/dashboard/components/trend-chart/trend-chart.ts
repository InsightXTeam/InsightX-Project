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

  private initChart() {
    this.chartOptions = {
      series: [],
      chart: {
        height: 350,
        type: 'line',
        zoom: { enabled: false },
        toolbar: { show: false }
      },
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth', width: 3 },
      title: { text: 'KPI Trend (Last 6 Months)', align: 'left' },
      xaxis: { categories: [] },
      colors: ['#0d6efd']
    };
  }

  private updateChart() {
    if (!this.trends || this.trends.length === 0) return;
    
    // Select the specified KPI or default to the first one
    let target = this.trends.find(t => t.kpiName === this.selectedKpiName);
    if (!target) target = this.trends[0];
    if (!target) return;

    this.chartOptions.series = [{
      name: target.kpiName,
      data: target.values
    }];
    
    this.chartOptions.xaxis = {
      categories: target.labels
    };
    
    this.chartOptions.title = {
      text: `${target.kpiName} Trend (Last 6 Months)`,
      align: 'left'
    };
  }
}
