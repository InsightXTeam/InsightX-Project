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
using System.Text;
using InsightX.API.Middleware;
using InsightX.Domain.Entities;
using InsightX.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();


            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

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

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter your JWT token only."
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<IKPIRepository, KPIRepository>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IDocumentReader, PdfDocumentReader>();
            builder.Services.AddScoped<IDocumentReader, ExcelReader>();
            builder.Services.AddScoped<IDocumentProcessor, DocumentProcessorService>();
            builder.Services.AddScoped<IAIExtractionService, OllamaSemanticKernelService>();
            builder.Services.AddScoped<IExtractedMetricRepository, ExtractedMetricRepository>();
            builder.Services.AddScoped<IDocumentReader, WordReader>();
            builder.Services.AddScoped<IDocumentReader, ImageReader>();

            var app = builder.Build();

            // Global exception handling
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Seed Roles & Super Admin
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.SeedAsync(
                    scope.ServiceProvider,
                    builder.Configuration);
            }

            await app.RunAsync();
        }
    }
}
