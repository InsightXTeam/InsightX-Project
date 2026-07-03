using InsightX.Application.Interfaces;

namespace InsightX.Application.Interfaces
{
    public interface IRagRetrievalService
    {
        Task<List<RagChunk>> RetrieveAsync(string question, int companyId, int limit = 5);
    }
}
