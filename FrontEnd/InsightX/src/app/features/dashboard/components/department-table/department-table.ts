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
}
