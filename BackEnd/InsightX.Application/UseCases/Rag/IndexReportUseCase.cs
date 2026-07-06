using InsightX.Application.Common;
using InsightX.Application.Interfaces;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;
using InsightXAI.Application.Interfaces.Rag;

namespace InsightXAI.Application.UseCases.Rag
{
    public class IndexReportUseCase : IIndexReportUseCase
    {
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly IReportRepository _reportRepository;

        public IndexReportUseCase(
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            IReportRepository reportRepository)
        {
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _reportRepository = reportRepository;
        }

        public async Task<ServiceResult<IndexReportResponseDto>> ExecuteAsync(
            IndexReportRequestDto request,
            int companyId,
            int? departmentId,
            string userId,
            string role,
            CancellationToken cancellationToken = default)
        {
            var currentYear = DateTime.UtcNow.Year;

            if (request.ReportId <= 0)
                return ServiceResult<IndexReportResponseDto>.Fail(400, "ReportId must be greater than zero.");
            if (request.Year < 2000 || request.Year > currentYear)
                return ServiceResult<IndexReportResponseDto>.Fail(400, $"Year must be between 2000 and {currentYear}.");
            if (string.IsNullOrWhiteSpace(request.Month))
                return ServiceResult<IndexReportResponseDto>.Fail(400, "Month is required.");
            if (string.IsNullOrWhiteSpace(request.FullText))
                return ServiceResult<IndexReportResponseDto>.Fail(400, "FullText is required.");

            var report = await _reportRepository.GetByIdAsync(request.ReportId);
            if (report == null)
                return ServiceResult<IndexReportResponseDto>.Fail(404, "Report not found.");
            
            if (report.CompanyId != companyId)
                return ServiceResult<IndexReportResponseDto>.Fail(403, "You do not have access to this report.");
                
            if (role != "Owner" && report.UploadedById != userId)
                return ServiceResult<IndexReportResponseDto>.Fail(403, "Only the owner or the uploader can index this report.");

            var chunks = _chunkingService.SplitIntoChunks(request.FullText);
            if (chunks.Count == 0)
            {
                return ServiceResult<IndexReportResponseDto>.Success(new IndexReportResponseDto
                {
                    ReportId = request.ReportId,
                    ChunksIndexed = 0
                });
            }

            var records = new List<VectorRecordDto>();
            var chunkTexts = chunks.Select(c => c.Text).ToList();
            
            int batchSize = 100;
            for (int i = 0; i < chunkTexts.Count; i += batchSize)
            {
                var batch = chunkTexts.Skip(i).Take(batchSize).ToList();
                var vectors = await _embeddingService.GetEmbeddingsAsync(batch, cancellationToken);
                
                for(int j = 0; j < batch.Count; j++)
                {
                    var absoluteIndex = i + j;
                    records.Add(new VectorRecordDto
                    {
                        Text = chunks[absoluteIndex].Text,
                        Vector = vectors[j],
                        CompanyId = companyId,
                        DepartmentId = departmentId ?? 0, // Fallback for vector store schema
                        ReportId = request.ReportId,
                        Month = request.Month,
                        Year = request.Year
                    });
                }
            }

            await _vectorStore.UpsertAsync(records, cancellationToken);

            return ServiceResult<IndexReportResponseDto>.Success(new IndexReportResponseDto
            {
                ReportId = request.ReportId,
                ChunksIndexed = records.Count
            });
        }
    }
}
