using System;
using System.Collections.Generic;

namespace InsightX.Application.DTOs
{
    public class DashboardKpiDto
    {
        public string Name { get; set; }
        public decimal CurrentValue { get; set; }
        public double Threshold { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public string ThresholdDirection { get; set; }
    }

    public class DashboardTrendDto
    {
        public string KPIName { get; set; }
        public List<decimal> Values { get; set; }
        public List<string> Labels { get; set; } 
    }

    public class DashboardDepartmentPerformanceDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int GoodKPIsCount { get; set; }
        public int WarningKPIsCount { get; set; }
        public int CriticalKPIsCount { get; set; }
        public string OverallStatus { get; set; }
    }
}
