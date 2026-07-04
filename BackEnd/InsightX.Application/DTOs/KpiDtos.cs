using InsightX.Domain.Enums;

namespace InsightX.Application.DTOs
{
    public record CreateKpiDto(string Name, double Threshold, string Unit, decimal AlertPercentageDiff, int TrendMonthsCount, ThresholdDirection ThresholdDirection);
    public record KpiResponseDto(int Id, string Name, double Threshold, string Unit, int CompanyId, decimal AlertPercentageDiff, int TrendMonthsCount, ThresholdDirection ThresholdDirection);
}
