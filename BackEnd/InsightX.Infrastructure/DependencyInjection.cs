using InsightX.Application.Common;
using InsightX.Application.Interfaces;
using InsightX.Infrastructure.Configuration;
using InsightX.Infrastructure.Services;
using InsightX.Infrastructure.Repositories;
using InsightX.Infrastructure.FileStorage;
using InsightX.Infrastructure.AI.Report;
using InsightX.Infrastructure.DocumentReaders;
using InsightX.Infrastructure.DocumentReaders.WordReader;
using InsightX.Infrastructure.DocumentReaders.Image;
using InsightX.Application.UseCases.Reports;
using InsightX.Application.UseCases.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsightX.Infrastructure
{
    public static class DependencyInjection
    {
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
                    .AllowAnyMethod();
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

            return services;
        }
    }
}
