using InsightX.Application.Common;
using InsightX.Application.Interfaces;
using InsightX.Infrastructure.Configuration;
using InsightX.Infrastructure.Services;
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

            return services;
        }
    }
}
