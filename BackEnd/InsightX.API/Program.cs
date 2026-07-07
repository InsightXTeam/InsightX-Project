
using InsightX.Infrastructure;
using InsightX.Infrastructure.AI.Rag;
using Asp.Versioning;
using InsightX.API.Middleware;
using InsightX.Domain.Entities;
using InsightX.Infrastructure;
using InsightX.Infrastructure.Configuration;
using InsightX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Alerts;
using InsightX.Infrastructure.AI;
using InsightX.Infrastructure.Anomaly;
using InsightX.Infrastructure.Anomaly.Rules;
using InsightX.Infrastructure.BackgroundServices;
using InsightX.Infrastructure.Handlers;
using InsightX.Infrastructure.Persistence.Repositories;
using Microsoft.SemanticKernel;

namespace InsightX.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
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



            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddMvc().AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
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

            /*
             * Anomaly Alert services
             * */
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();
            builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();
            builder.Services.AddHttpClient<IAlertMessageGenerator, OllamaAlertGenerator>();

            builder.Services.AddScoped<IAnomalyRule, ThresholdRule>();
            builder.Services.AddScoped<IAnomalyRule, YearOverYearRule>();
            builder.Services.AddScoped<IAnomalyRule, TrendRule>();

            builder.Services.AddScoped<IAnomalyDetector, ThreeLevelAnomalyDetector>();
            builder.Services.AddScoped<IReportConfirmedHandler, ReportConfirmedHandler>();

            builder.Services.AddScoped<IGenerateAlertUseCase, GenerateAlertUseCase>();
            builder.Services.AddScoped<IGetAlertsUseCase, GetAlertsUseCase>();
            builder.Services.AddScoped<IMarkAlertSeenUseCase, MarkAlertSeenUseCase>();
            builder.Services.AddScoped<ICreateMonthlyReminderUseCase, CreateMonthlyReminderUseCase>();

            builder.Services.AddScoped<InsightX.Application.Interfaces.IDashboardService, InsightX.Infrastructure.Services.DashboardService>();

            builder.Services.AddHostedService<MonthlyReminderBackgroundService>();

            builder.Services.AddRagInfrastructure(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // Fail-fast guard: prevent production startup with placeholder secrets
            if (!app.Environment.IsDevelopment())
            {
                var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
                var adminEmail = builder.Configuration["SuperAdmin:Email"] ?? "";
                var adminPassword = builder.Configuration["SuperAdmin:Password"] ?? "";
                if (jwtKey.Contains("CHANGE_ME") || adminEmail.Contains("CHANGE_ME") || adminPassword.Contains("CHANGE_ME"))
                {
                    throw new InvalidOperationException(
                        "SECURITY: Production startup blocked. Jwt:Key and SuperAdmin credentials " +
                        "must be overridden via environment variables or user-secrets. " +
                        "See appsettings.json for details.");
                }
            }


            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(CorsSettings.SectionName);

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.SeedAsync(
                    scope.ServiceProvider,
                    builder.Configuration);

                await scope.ServiceProvider.InitializeRagInfrastructureAsync();
            }

            await app.RunAsync();
        }
    }
}

