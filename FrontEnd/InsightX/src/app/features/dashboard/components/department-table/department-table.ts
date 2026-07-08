import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardDepartmentPerformanceDto } from '../../dashboard.service';

@Component({
  selector: 'app-department-table',
  imports: [CommonModule],
  templateUrl: './department-table.html',
  styleUrl: './department-table.css'
})
export class DepartmentTable {
  @Input() departments: DashboardDepartmentPerformanceDto[] = [];

  exportToFile() {
    if (!this.departments || this.departments.length === 0) return;

    // CSV Header
    const headers = ['Department', 'Good KPIs', 'Warning KPIs', 'Critical KPIs', 'Overall Status'];
    
    // CSV Rows
    const rows = this.departments.map(d => [
      `"${d.departmentName.replace(/"/g, '""')}"`,
      d.goodKPIsCount,
      d.warningKPIsCount,
      d.criticalKPIsCount,
      `"${d.overallStatus.toUpperCase()}"`
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map(e => e.join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `department_performance_report_${new Date().toISOString().slice(0, 10)}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
