using InsightX.Infrastructure.AI.Rag;
using InsightXAI.Application.Interfaces;
using InsightXAI.Application.Interfaces.Rag;
using InsightXAI.Application.UseCases.Rag;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using InsightX.Application.Common;
using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Documents;
using InsightX.Application.UseCases.Reports;
using InsightX.Infrastructure.AI.Report;
using InsightX.Infrastructure.Configuration;
using InsightX.Infrastructure.DocumentReaders;
using InsightX.Infrastructure.DocumentReaders.Image;
using InsightX.Infrastructure.DocumentReaders.WordReader;
using InsightX.Infrastructure.FileStorage;
using InsightX.Infrastructure.Repositories;
using InsightX.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using InsightX.Infrastructure.AI.Agent;

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

            services.AddScoped<IIndexReportUseCase, IndexReportUseCase>();
            services.AddScoped<IRetrieveChunksUseCase, RetrieveChunksUseCase>();
            services.AddScoped<IDeleteReportChunksUseCase, DeleteReportChunksUseCase>();

            return services;
        }

        public static async Task InitializeRagInfrastructureAsync(this IServiceProvider provider)
        {
            try
            {
                var qdrant = provider.GetRequiredService<QdrantClient>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                await QdrantInitializer.InitializeAsync(qdrant, configuration);
            }
            catch (Exception ex)
            {
                // Log and swallow the exception to prevent crash loops
                var logger = provider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("RagInfrastructure");
                if (logger != null)
                {
                    logger.LogError(ex, "Failed to initialize Qdrant: {Message}", ex.Message);
                }
            }
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSettings = configuration
                .GetSection(CorsSettings.SectionName)
                .Get<CorsSettings>()!;

            services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

            services.AddCors(options =>
            {
                options.AddPolicy(CorsSettings.SectionName, policy =>
                {
                    policy
                    .WithOrigins(corsSettings.AllowedOrigins.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            services.AddScoped<ITokenService, TokenService>();
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IKpiService, KpiService>();

            // Reports Feature Registrations
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IExtractedMetricRepository, ExtractedMetricRepository>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IDocumentProcessor, DocumentProcessorService>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            // Register HttpClient for AI Service
            services.AddHttpClient<IAIExtractionService, OllamaSemanticKernelService>();
            services.AddScoped<IAIExtractionService, OllamaSemanticKernelService>();

            // Register all Document Readers
            services.AddScoped<IDocumentReader, ExcelReader>();
            services.AddScoped<IDocumentReader, PdfDocumentReader>();
            services.AddScoped<IDocumentReader, WordReader>();
            services.AddScoped<IDocumentReader, ImageReader>();

            // Register AI Agent Services
            services.AddHttpClient<IChatCompletionService, ItiChatCompletionService>((sp, client) => { });
            services.AddScoped<IChatCompletionService, ItiChatCompletionService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var alertAiConfig = config.GetSection("AlertAI");
                
                var chatApiUrl = alertAiConfig["ChatEndpoint"] ?? throw new ArgumentNullException("AlertAI:ChatEndpoint is required");
                var apiKey = alertAiConfig["ApiKey"] ?? throw new ArgumentNullException("AlertAI:ApiKey is required");
                var modelId = alertAiConfig["ModelId"] ?? throw new ArgumentNullException("AlertAI:ModelId is required");
                
                var httpClient = httpClientFactory.CreateClient();
                return new ItiChatCompletionService(httpClient, chatApiUrl, apiKey, modelId);
            });
            services.AddScoped<IAgentService, AgentService>();

            return services;
        }
    }
}
