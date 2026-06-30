namespace InsightX.Application.DTOs
{
    public record CreateKpiDto(string Name, double Threshold, string Unit);
    public record KpiResponseDto(int Id, string Name, double Threshold, string Unit, int CompanyId);
}
