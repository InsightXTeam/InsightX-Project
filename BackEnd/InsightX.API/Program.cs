
using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Alerts;
using InsightX.Infrastructure.AI;
using InsightX.Infrastructure.Anomaly;
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
            builder.Services.AddScoped<IAnomalyDetector, ThreeLevelAnomalyDetector>();
            builder.Services.AddScoped<IAlertMessageGenerator, SemanticKernelAlertGenerator>();

            builder.Services.AddScoped<GenerateAlertUseCase>();
            builder.Services.AddScoped<GetAlertsUseCase>();
            builder.Services.AddScoped<MarkAlertSeenUseCase>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
