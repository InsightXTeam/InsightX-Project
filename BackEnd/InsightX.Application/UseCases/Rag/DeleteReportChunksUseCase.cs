using InsightX.Application.Common;
using InsightX.Application.Interfaces;
using InsightXAI.Application.Interfaces;
using InsightXAI.Application.Interfaces.Rag;

namespace InsightXAI.Application.UseCases.Rag
{
    public class DeleteReportChunksUseCase : IDeleteReportChunksUseCase
    {
        private readonly IVectorStore _vectorStore;
        private readonly IReportRepository _reportRepository;

        public DeleteReportChunksUseCase(IVectorStore vectorStore, IReportRepository reportRepository)
        {
            _vectorStore = vectorStore;
            _reportRepository = reportRepository;
        }

        public async Task<ServiceResult> ExecuteAsync(int companyId, int reportId, string userId, string role, CancellationToken cancellationToken = default)
        {
            if (reportId <= 0)
                return ServiceResult.Fail(400, "ReportId must be greater than zero.");

            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
                return ServiceResult.Fail(404, "Report not found.");
            
            if (report.CompanyId != companyId)
                return ServiceResult.Fail(403, "You do not have access to this report.");
                
            if (role != "Owner" && report.UploadedById != userId)
                return ServiceResult.Fail(403, "Only the owner or the uploader can delete this report's chunks.");

            await _vectorStore.DeleteByReportIdAsync(companyId, reportId, cancellationToken);
            return ServiceResult.Success();
        }
    }
}
