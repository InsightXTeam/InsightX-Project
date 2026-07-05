using InsightX.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsightX.Application.DTOs
{
    public record CreateKpiDto(
        [Required, MaxLength(100)] string Name,
        [Range(0, double.MaxValue)] double Threshold,
        [Required, MaxLength(50)] string Unit,
        [Range(0, 100)] decimal AlertPercentageDiff,
        [Range(1, 120)] int TrendMonthsCount,
        ThresholdDirection ThresholdDirection
    );
    public record UpdateKpiDto(
    [Required, MaxLength(100)] string Name,
    [Range(0, double.MaxValue)] double Threshold,
    [Required, MaxLength(50)] string Unit,
    [Range(0, 100)] decimal AlertPercentageDiff,
    [Range(1, 120)] int TrendMonthsCount,
    ThresholdDirection ThresholdDirection
);

    public record KpiResponseDto(int Id, string Name, double Threshold, string Unit, int CompanyId, decimal AlertPercentageDiff, int TrendMonthsCount, ThresholdDirection ThresholdDirection);
}
