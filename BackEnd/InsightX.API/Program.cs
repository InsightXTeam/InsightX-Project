
using InsightX.Infrastructure;
using InsightX.Infrastructure.AI.Rag;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;

namespace InsightX.API
{
    public class Program
    {
        public static async Task Main(string[] args)
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

            builder.Services.AddRagInfrastructure(builder.Configuration);

            var app = builder.Build();

            // Ensure the Qdrant collection exists before serving requests.
            using (var scope = app.Services.CreateScope())
            {
                var qdrant = scope.ServiceProvider.GetRequiredService<QdrantClient>();

                await QdrantInitializer.InitializeAsync(qdrant);
            }

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
