using System;
using System.Collections.Generic;

namespace InsightX.Application.DTOs
{
    public record KpiSetupDto(string Name, double Threshold, string Unit);
    public record SetupDto(List<KpiSetupDto> KPIs);

    public record CompanyProfileDto(int Id, string Name, DateTime CreatedAt, List<DepartmentProfileDto> Departments, List<KpiProfileDto> KPIs);
    public record DepartmentProfileDto(int Id, string Name);
    public record KpiProfileDto(int Id, string Name, double Threshold, string Unit);
}
