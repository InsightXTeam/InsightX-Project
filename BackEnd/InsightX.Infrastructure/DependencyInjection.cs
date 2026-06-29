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
                    config["RagModel:ApiKey"]!,
                    config["RagModel:Endpoint"]!,
                    config["RagModel:EmbeddingModel"]!);
            });


            services.AddSingleton(new QdrantClient(
                host: configuration["Qdrant:Host"]!,
                port: int.Parse(configuration["Qdrant:Port"]!)));

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
