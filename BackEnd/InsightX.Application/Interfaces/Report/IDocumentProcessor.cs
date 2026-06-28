namespace InsightX.Application.Interfaces
{
    public interface IDocumentProcessor
    {
        Task ProcessAsync(int reportId);
        Task ExtractKpisAsync(int reportId);
    }
}