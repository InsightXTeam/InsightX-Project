using InsightXAI.Application.Interfaces;
using Microsoft.SemanticKernel.Embeddings;

namespace Insight_test.All.Services
{
    /// <summary>
    /// Generates embeddings using Semantic Kernel.
    /// </summary>
    public class EmbeddingService : IEmbeddingService
    {
        // Semantic Kernel embedding generator.
        private readonly ITextEmbeddingGenerationService _embeddingGenerator;

        // The embedding generator is injected, not created here.
        public EmbeddingService(ITextEmbeddingGenerationService embeddingGenerator)
        {
            _embeddingGenerator = embeddingGenerator;
        }

        public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            return embedding.ToArray();
        }

        public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default)
        {
            var embeddings = await _embeddingGenerator.GenerateEmbeddingsAsync(texts, cancellationToken: cancellationToken);
            return embeddings.Select(e => e.ToArray()).ToList();
        }
    }
}
