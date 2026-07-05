namespace InsightX.Infrastructure.Configuration
{
    public sealed class CorsSettings
    {
        public const string SectionName = "Cors";

        public List<string> AllowedOrigins { get; init; } = [];
    }
}
