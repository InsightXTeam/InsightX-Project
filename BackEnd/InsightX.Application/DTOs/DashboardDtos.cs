using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InsightX.Application.DTOs
{
    public class DashboardKpiDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("currentValue")]
        public decimal CurrentValue { get; set; }
        [JsonPropertyName("threshold")]
        public double Threshold { get; set; }
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("thresholdDirection")]
        public string ThresholdDirection { get; set; }
    }

    public class DashboardTrendDto
    {
        [JsonPropertyName("kpiName")]
        public string KPIName { get; set; }
        [JsonPropertyName("values")]
        public List<decimal> Values { get; set; }
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } 
    }

    public class DashboardDepartmentPerformanceDto
    {
        [JsonPropertyName("departmentId")]
        public int DepartmentId { get; set; }
        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; }
        [JsonPropertyName("goodKPIsCount")]
        public int GoodKPIsCount { get; set; }
        [JsonPropertyName("warningKPIsCount")]
        public int WarningKPIsCount { get; set; }
        [JsonPropertyName("criticalKPIsCount")]
        public int CriticalKPIsCount { get; set; }
        [JsonPropertyName("overallStatus")]
        public string OverallStatus { get; set; }
    }
}
