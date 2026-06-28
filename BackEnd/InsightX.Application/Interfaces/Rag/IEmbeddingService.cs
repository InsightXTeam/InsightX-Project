namespace InsightXAI.Application.Interfaces
{
    /// <summary>
    /// Generates vector embeddings for text content.
    /// </summary>
    public interface IEmbeddingService
    {
        // Generates an embedding for a single text input (e.g. one question).
        Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);

        // Generates embeddings for multiple text inputs.
        Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default);
    }
}
