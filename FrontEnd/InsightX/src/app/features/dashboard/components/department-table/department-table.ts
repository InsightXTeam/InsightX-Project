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

    // Determine column widths
    const colWidths = [
      Math.max(20, ...this.departments.map(d => d.departmentName.length)), // Department
      15, // Good KPIs
      15, // Warning KPIs
      15, // Critical KPIs
      15  // Status
    ];

    const pad = (text: string | number, width: number) => String(text).padEnd(width, ' ');

    let txtContent = '================================================================================\n';
    txtContent += '                            DEPARTMENT PERFORMANCE REPORT\n';
    txtContent += '================================================================================\n\n';

    // Headers
    txtContent += pad('DEPARTMENT', colWidths[0]) + ' | ' + 
                  pad('GOOD KPIs', colWidths[1]) + ' | ' + 
                  pad('WARNING KPIs', colWidths[2]) + ' | ' + 
                  pad('CRITICAL KPIs', colWidths[3]) + ' | ' + 
                  pad('STATUS', colWidths[4]) + '\n';
    
    txtContent += '-'.repeat(colWidths.reduce((a, b) => a + b, 0) + 12) + '\n';

    // Rows
    this.departments.forEach(d => {
      txtContent += pad(d.departmentName, colWidths[0]) + ' | ' + 
                    pad(d.goodKPIsCount, colWidths[1]) + ' | ' + 
                    pad(d.warningKPIsCount, colWidths[2]) + ' | ' + 
                    pad(d.criticalKPIsCount, colWidths[3]) + ' | ' + 
                    pad(d.overallStatus.toUpperCase(), colWidths[4]) + '\n';
    });
    
    txtContent += '\n================================================================================\n';

    const blob = new Blob([txtContent], { type: 'text/plain;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', 'department_performance_report.txt');
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
