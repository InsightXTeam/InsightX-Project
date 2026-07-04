using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;

namespace InsightXAI.Application.UseCases.Rag
{
    /// <summary>
    /// Handles report indexing into the vector store.
    /// </summary>
    public class IndexReportUseCase
    {
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;

        public IndexReportUseCase(
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            IVectorStore vectorStore)
        {
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
        }

        public async Task<IndexReportResponseDto> ExecuteAsync(
            IndexReportRequestDto request,
               int companyId,
               int departmentId,
            CancellationToken cancellationToken = default)
        {
            var currentYear = DateTime.UtcNow.Year;

            if (request.ReportId <= 0)
                throw new ArgumentException("ReportId must be greater than zero.", nameof(request.ReportId));

            if (request.Year < 2000 || request.Year > currentYear)
                throw new ArgumentException($"Year must be between 2000 and {currentYear}.", nameof(request.Year));

            if (string.IsNullOrWhiteSpace(request.Month))
                throw new ArgumentException("Month is required.", nameof(request.Month));

            if (string.IsNullOrWhiteSpace(request.FullText))
                throw new ArgumentException("FullText is required.", nameof(request.FullText));

            // Split report text into chunks.
            var chunks = _chunkingService.SplitIntoChunks(request.FullText);

            // Return early if there is no content to index.
            if (chunks.Count == 0)
            {
                return new IndexReportResponseDto
                {
                    ReportId = request.ReportId,
                    ChunksIndexed = 0
                };
            }


            var chunkTexts = chunks.Select(c => c.Text).ToList();
            // Generate embeddings for all chunks.
            var vectors = await _embeddingService.GetEmbeddingsAsync(chunkTexts, cancellationToken);

            if (vectors.Count != chunks.Count)
                throw new InvalidOperationException($"Embedding service returned {vectors.Count} vectors for {chunks.Count} chunks.");

            // Build vector records with report metadata.
            var records = new List<VectorRecordDto>();
            for (int i = 0; i < chunks.Count; i++)
            {
                records.Add(new VectorRecordDto
                {
                    Text = chunks[i].Text,
                    Vector = vectors[i],
                    CompanyId = companyId,
                    DepartmentId = departmentId,
                    ReportId = request.ReportId,
                    Month = request.Month,
                    Year = request.Year
                });
            }

            // Store vectors in the vector database.
            await _vectorStore.UpsertAsync(records, cancellationToken);

            return new IndexReportResponseDto
            {
                ReportId = request.ReportId,
                ChunksIndexed = records.Count
            };
        }
    }
}
