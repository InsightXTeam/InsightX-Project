
using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Alerts;
using InsightX.Infrastructure.AI;
using InsightX.Infrastructure.Anomaly;
using InsightX.Infrastructure.Anomaly.Rules;
using InsightX.Infrastructure.BackgroundServices;
using InsightX.Infrastructure.Handlers;
using InsightX.Infrastructure.Persistence;
using InsightX.Infrastructure.Persistence.Repositories;
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

            /*
             * Anomaly Alert services
             * */
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();
            builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();
            builder.Services.AddScoped<IAlertMessageGenerator, SemanticKernelAlertGenerator>();

            // Register all 3 anomaly rules — order matters (Threshold → ZScore → Trend)
            builder.Services.AddScoped<IAnomalyRule, ThresholdRule>();
            builder.Services.AddScoped<IAnomalyRule, ZScoreRule>();
            builder.Services.AddScoped<IAnomalyRule, TrendRule>();

            builder.Services.AddScoped<IAnomalyDetector, ThreeLevelAnomalyDetector>();
            builder.Services.AddScoped<IReportConfirmedHandler, ReportConfirmedHandler>();

            // Use cases
            builder.Services.AddScoped<GenerateAlertUseCase>();
            builder.Services.AddScoped<GetAlertsUseCase>();
            builder.Services.AddScoped<MarkAlertSeenUseCase>();
            builder.Services.AddScoped<CreateMonthlyReminderUseCase>();

            // Monthly reminder: notifies company owners to upload their report on the 25th of each month
            builder.Services.AddHostedService<MonthlyReminderBackgroundService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // UseAuthentication MUST come before UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

