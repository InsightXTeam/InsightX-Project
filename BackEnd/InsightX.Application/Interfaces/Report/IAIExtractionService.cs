namespace InsightX.Application.Interfaces
{
    public interface IAIExtractionService
    {
        Task<string> ExtractMetricsAsync(string text, IEnumerable<string> predefinedKPIs);

        Task<string> ExtractTextFromImageAsync(byte[] imageBytes, string fileName);
    }
}