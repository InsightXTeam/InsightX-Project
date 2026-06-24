namespace InsightX.Application.Interfaces
{
    public interface IAIExtractionService
    {
        Task<string> ExtractMetricsAsync(string text);
    }
}