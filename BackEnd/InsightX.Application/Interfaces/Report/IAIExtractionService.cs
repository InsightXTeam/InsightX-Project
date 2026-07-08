using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IAIExtractionService
    {
        Task<string> ExtractMetricsAsync(string text, IEnumerable<KpiResponseDto> predefinedKPIs, string? departmentName = null);

        Task<string> ExtractTextFromImageAsync(byte[] imageBytes, string fileName);
    }
}