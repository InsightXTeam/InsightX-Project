using System;
using System.Collections.Generic;

using InsightX.Domain.Enums;

namespace InsightX.Application.DTOs
{
    public record KpiSetupDto(string Name, double Threshold, string Unit, decimal AlertPercentageDiff, int TrendMonthsCount, ThresholdDirection ThresholdDirection);
    public record SetupDto(List<KpiSetupDto> KPIs);

    public record CompanyProfileDto(int Id, string Name, DateTime CreatedAt, List<DepartmentProfileDto> Departments, List<KpiProfileDto> KPIs);
    public record DepartmentProfileDto(int Id, string Name);
    public record KpiProfileDto(int Id, string Name, double Threshold, string Unit, decimal AlertPercentageDiff, int TrendMonthsCount, ThresholdDirection ThresholdDirection);
}
