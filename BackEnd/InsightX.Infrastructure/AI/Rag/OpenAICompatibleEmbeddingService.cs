using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// Custom Semantic Kernel embedding service that works with
// any OpenAI-compatible Embeddings API (OpenRouter, Together AI, etc.).
public sealed class OpenAICompatibleEmbeddingService : ITextEmbeddingGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    // Required by Semantic Kernel's IAIService interface.
    public IReadOnlyDictionary<string, object?> Attributes { get; }
        = new Dictionary<string, object?>();

    public OpenAICompatibleEmbeddingService(
        HttpClient httpClient,
        string apiKey,
        string endpoint,
        string model)
    {
        _httpClient = httpClient;

        // Configure provider endpoint and authentication.
        _httpClient.BaseAddress = new Uri(endpoint);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _model = model;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string data,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Reuse the batch implementation for a single text.
        var embeddings = await GenerateEmbeddingsAsync(
            new[] { data },
            kernel,
            cancellationToken);

        return embeddings.First();
    }

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> data,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Build the standard OpenAI-compatible embedding request.
        var request = new
        {
            model = _model,
            input = data
        };

        var json = JsonSerializer.Serialize(request);

        // Send the embedding request to the configured provider.
        var response = await _httpClient.PostAsync(
            "embeddings",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        // Parse the embedding vectors from the provider response.
        using var document = JsonDocument.Parse(body);

        var result = new List<ReadOnlyMemory<float>>();

        foreach (var item in document.RootElement
                     .GetProperty("data")
                     .EnumerateArray())
        {
            var vector = item
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            result.Add(vector);
        }

        return result;
    }
}