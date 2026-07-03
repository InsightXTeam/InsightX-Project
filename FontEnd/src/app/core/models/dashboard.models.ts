export interface KpiCard {
  kpiName: string;
  currentValue: number;
  threshold: number;
  isPercentage: boolean;
  status: 'Good' | 'Critical';
}

export interface TrendPoint {
  month: string;
  year: number;
  value: number;
}

export interface DepartmentSummary {
  departmentId: number;
  departmentName: string;
  kpiName: string;
  currentValue: number;
  threshold: number;
  status: 'Good' | 'Critical';
}

export interface DashboardAlert {
  id: number;
  kpiName: string;
  currentValue: number;
  threshold: number;
  message: string;
  recommendation: string;
  seenByOwner: boolean;
  createdAt: string;
  departmentName: string;
}
