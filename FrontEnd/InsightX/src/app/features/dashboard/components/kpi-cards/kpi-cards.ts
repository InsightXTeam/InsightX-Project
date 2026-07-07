import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardKpiDto } from '../../dashboard.service';

@Component({
  selector: 'app-kpi-cards',
  imports: [CommonModule],
  templateUrl: './kpi-cards.html',
  styleUrl: './kpi-cards.css'
})
export class KpiCards {
  @Input() kpis: DashboardKpiDto[] = [];
}
