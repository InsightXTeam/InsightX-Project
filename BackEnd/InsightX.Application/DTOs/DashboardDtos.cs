using System;

namespace InsightX.Application.DTOs
{
    public class KpiCardDto
    {
        public string KPIName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public bool IsPercentage { get; set; }
        public string Status { get; set; } = "Good"; // "Good" (green), "Critical" (red)
    }

    public class TrendChartDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public double Value { get; set; }
    }

    public class DepartmentSummaryDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string KPIName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public string Status { get; set; } = "Good";
    }

    public class AlertDto
    {
        public int Id { get; set; }
        public string KPIName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public bool SeenByOwner { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }
}
