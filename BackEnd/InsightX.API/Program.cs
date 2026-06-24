using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Documents;
using InsightX.Application.UseCases.Reports;
using InsightX.Infrastructure.AI;
using InsightX.Infrastructure.DocumentReaders;
using InsightX.Infrastructure.DocumentReaders.Image;
using InsightX.Infrastructure.DocumentReaders.WordReader;
using InsightX.Infrastructure.FileStorage;
using InsightX.Infrastructure.Persistence;
using InsightX.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InsightX.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connection);
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("angular",
                    policy =>
                    {
                        policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("angular");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}