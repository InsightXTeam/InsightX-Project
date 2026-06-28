using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using InsightX.Application.Common;
using InsightX.Infrastructure.Services;
using InsightX.Application.Interfaces.Auth;

namespace InsightX.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
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
