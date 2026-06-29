using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Documents;
using InsightX.Application.UseCases.Reports;
using InsightX.Infrastructure.DocumentReaders;
using InsightX.Infrastructure.DocumentReaders.Image;
using InsightX.Infrastructure.DocumentReaders.WordReader;
using InsightX.Infrastructure.FileStorage;
using InsightX.Infrastructure.Persistence;
using InsightX.Infrastructure.Repositories;
using InsightX.Infrastructure.AI.Report;
using Microsoft.EntityFrameworkCore;

namespace InsightX.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add services to the container.
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 104857600; // 100MB
            });

            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 104857600; // 100MB
                options.MemoryBufferThreshold = int.MaxValue;
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connection);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(origin => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "InsightX API",
                    Version = "v1"
                });
            });

            // Report Upload Services (Person 2)
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IDocumentReader, PdfDocumentReader>();
            builder.Services.AddScoped<IDocumentReader, ExcelReader>();
            builder.Services.AddScoped<IDocumentProcessor, DocumentProcessorService>();
            builder.Services.AddScoped<IAIExtractionService, OllamaSemanticKernelService>();
            builder.Services.AddScoped<IExtractedMetricRepository, ExtractedMetricRepository>();
            builder.Services.AddScoped<IDocumentReader, WordReader>();
            builder.Services.AddScoped<IDocumentReader, ImageReader>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
