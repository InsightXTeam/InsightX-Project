using System;
using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace InsightX.Infrastructure.AI
{
    public class SemanticKernelEmbeddingService : IEmbeddingService
    {
        private readonly ILogger<SemanticKernelEmbeddingService> _logger;
        private readonly ITextEmbeddingGenerationService? _embeddingService;

        public bool IsConfigured => _embeddingService != null;

        public SemanticKernelEmbeddingService(
            IConfiguration configuration,
            ILogger<SemanticKernelEmbeddingService> logger)
        {
            _logger = logger;

            try
            {
                var kernelBuilder = Kernel.CreateBuilder();

                var azureDeployment = configuration["AzureOpenAI:EmbeddingDeploymentName"];
                var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
                var azureApiKey = configuration["AzureOpenAI:ApiKey"];

                if (!string.IsNullOrEmpty(azureDeployment) &&
                    !string.IsNullOrEmpty(azureEndpoint) &&
                    !string.IsNullOrEmpty(azureApiKey))
                {
                    kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(azureDeployment, azureEndpoint, azureApiKey);
                }
                else
                {
                    var openAiModel = configuration["OpenAI:EmbeddingModelId"] ?? "text-embedding-ada-002";
                    var openAiApiKey = configuration["OpenAI:ApiKey"];

                    if (!string.IsNullOrEmpty(openAiApiKey))
                    {
                        kernelBuilder.AddOpenAITextEmbeddingGeneration(openAiModel, openAiApiKey);
                    }
                }

                var kernel = kernelBuilder.Build();
                _embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

                if (_embeddingService != null)
                {
                    _logger.LogInformation("Embedding service configured successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Embedding service not configured. Qdrant search will use fallback mode.");
            }
        }

        public async Task<float[]?> GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            if (_embeddingService == null || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            try
            {
                var result = await _embeddingService.GenerateEmbeddingAsync(text, kernel: null, cancellationToken);
                return result.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding.");
                return null;
            }
        }
    }
}
