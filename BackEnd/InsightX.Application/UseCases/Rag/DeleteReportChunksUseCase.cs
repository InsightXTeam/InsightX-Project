using InsightXAI.Application.Interfaces;

namespace InsightXAI.Application.UseCases.Rag
{
    /// <summary>
    /// Removes all indexed chunks associated with a report.
    /// </summary>
    public class DeleteReportChunksUseCase
    {
        private readonly IVectorStore _vectorStore;

        public DeleteReportChunksUseCase(IVectorStore vectorStore)
        {
            _vectorStore = vectorStore;
        }

        public async Task ExecuteAsync(int companyId, int reportId, CancellationToken cancellationToken = default)
        {
            if (reportId <= 0)
                throw new ArgumentException("ReportId must be greater than zero.", nameof(reportId));

            // Delete report vectors from the vector store.
            await _vectorStore.DeleteByReportIdAsync(companyId, reportId, cancellationToken);
        }
    }
}
