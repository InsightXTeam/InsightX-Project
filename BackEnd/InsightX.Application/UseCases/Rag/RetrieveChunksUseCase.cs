using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;

namespace InsightXAI.Application.UseCases.Rag
{
    /// <summary>
    /// Retrieves the most relevant chunks for a user question.
    /// </summary>
    public class RetrieveChunksUseCase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;

        public RetrieveChunksUseCase(IEmbeddingService embeddingService, IVectorStore vectorStore)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
        }

        public async Task<RetrieveResponseDto> ExecuteAsync(RetrieveRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Generate an embedding for the question.
            var questionVector = await _embeddingService.GetEmbeddingAsync(request.Question, cancellationToken);

            // Search for the most relevant chunks within the company scope.
            var chunks = await _vectorStore.SearchAsync(
                questionVector,
                request.CompanyId,
                request.TopK,
                cancellationToken);

            return new RetrieveResponseDto
            {
                Chunks = chunks
            };
        }
    }
}
