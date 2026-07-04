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
            int companyId,
            CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrWhiteSpace(request.Question))
                throw new ArgumentException("Question is required.", nameof(request.Question));

            if (request.TopK <= 0)
                throw new ArgumentException("TopK must be greater than zero.", nameof(request.TopK));

            if (request.TopK > 50)
                throw new ArgumentException("TopK cannot be greater than 50.", nameof(request.TopK));

            // Generate an embedding for the question.
            var questionVector = await _embeddingService.GetEmbeddingAsync(request.Question, cancellationToken);

            // Search for the most relevant chunks within the company scope.
            var chunks = await _vectorStore.SearchAsync(
                questionVector,
                companyId,
                request.TopK,
                cancellationToken);

            return new RetrieveResponseDto
            {
                Chunks = chunks
            };
        }
    }
}
