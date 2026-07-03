using InsightX.Application.Interfaces;
using InsightX.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InsightX.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
