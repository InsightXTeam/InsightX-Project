using InsightX.Infrastructure.AI.Rag;
using InsightXAI.Application.Interfaces;
using InsightXAI.Application.UseCases.Rag;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;

namespace InsightX.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRagInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {

            services.AddHttpClient();
            services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                return new OpenAICompatibleEmbeddingService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(),

                    config["RagModel:ApiKey"] ?? throw new InvalidOperationException("Missing configuration: RagModel:ApiKey"),
                    config["RagModel:Endpoint"] ?? throw new InvalidOperationException("Missing configuration: RagModel:Endpoint"),
                    config["RagModel:EmbeddingModel"] ?? throw new InvalidOperationException("Missing configuration: RagModel:EmbeddingModel"));
            });

            var qdrantHost = configuration["Qdrant:Host"] ?? throw new InvalidOperationException("Missing configuration: Qdrant:Host");

            var qdrantPort = configuration.GetValue<int>("Qdrant:Port");

            var qdrantApiKey = configuration["Qdrant:ApiKey"] ?? throw new InvalidOperationException("Missing configuration: Qdrant:ApiKey");

            services.AddSingleton(new QdrantClient(
                host: qdrantHost,
                port: qdrantPort,
                https: true,
                apiKey: qdrantApiKey));

            services.AddScoped<IChunkingService, ChunkingService>();
            services.AddScoped<IEmbeddingService, EmbeddingService>();
            services.AddScoped<IVectorStore, QdrantVectorStore>();

            services.AddScoped<IndexReportUseCase>();
            services.AddScoped<RetrieveChunksUseCase>();
            services.AddScoped<DeleteReportChunksUseCase>();

            return services;
        }
    }
}
