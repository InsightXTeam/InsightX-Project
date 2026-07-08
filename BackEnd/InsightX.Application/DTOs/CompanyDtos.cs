using InsightX.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsightX.Application.DTOs
{
    public record KpiSetupDto(
      [Required, MaxLength(100)] string Name,
      [Range(0, double.MaxValue)] double Threshold,
      [Required, MaxLength(50)] string Unit,
      [Range(0, 100)] decimal AlertPercentageDiff,
      [Range(1, 120)] int TrendMonthsCount,
      ThresholdDirection ThresholdDirection,
      int? DepartmentId = null,
      [MaxLength(500)] string? Description = null
  );
    public record SetupDto([Required, MinLength(1)] List<KpiSetupDto> KPIs);

    public record CompanyProfileDto(int Id, string Name, DateTime CreatedAt, List<DepartmentProfileDto> Departments, List<KpiProfileDto> KPIs);
    public record DepartmentProfileDto(int Id, string Name);
    public record KpiProfileDto(int Id, string Name, double Threshold, string Unit, decimal AlertPercentageDiff, int TrendMonthsCount, ThresholdDirection ThresholdDirection, int? DepartmentId = null, string? DepartmentName = null, string? Description = null);
}
