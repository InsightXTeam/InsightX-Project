using InsightX.Application.Common;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;
using InsightXAI.Application.Interfaces.Rag;

namespace InsightXAI.Application.UseCases.Rag
{
    public class RetrieveChunksUseCase : IRetrieveChunksUseCase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;

        public RetrieveChunksUseCase(IEmbeddingService embeddingService, IVectorStore vectorStore)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
        }

        public async Task<ServiceResult<RetrieveResponseDto>> ExecuteAsync(RetrieveRequestDto request,
            int companyId,
            int? departmentId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return ServiceResult<RetrieveResponseDto>.Fail(400, "Question is required.");
            if (request.TopK <= 0 || request.TopK > 50)
                return ServiceResult<RetrieveResponseDto>.Fail(400, "TopK must be between 1 and 50.");

            var questionVector = await _embeddingService.GetEmbeddingAsync(request.Question, cancellationToken);

            var chunks = await _vectorStore.SearchAsync(
                questionVector,
                companyId,
                request.TopK,
                departmentId,
                cancellationToken);

            return ServiceResult<RetrieveResponseDto>.Success(new RetrieveResponseDto
            {
                Chunks = chunks
            });
        }
    }
}
