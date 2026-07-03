using InsightX.Application.Interfaces;
using InsightX.Infrastructure.AI;
using InsightX.Infrastructure.Persistence;
using InsightX.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsightX.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connection = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connection);
            });

            services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<ILlmChatService, SemanticKernelLlmService>();
            services.AddScoped<IEmbeddingService, SemanticKernelEmbeddingService>();
            services.AddHttpClient<IRagRetrievalService, RagRetrievalService>();
            services.AddHttpClient<IRagService, RagService>();

            return services;
        }
    }
}
