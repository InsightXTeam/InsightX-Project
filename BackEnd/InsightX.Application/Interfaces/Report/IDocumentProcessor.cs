namespace InsightX.Application.Interfaces
{
    public interface IDocumentProcessor
    {
        Task ProcessAsync(int reportId, int companyId);

        Task ExtractKpisAsync(int reportId, int companyId);
    }
}