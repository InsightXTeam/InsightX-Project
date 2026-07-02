using InsightX.Application.Interfaces;
using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InsightX.Infrastructure.AI
{
    public class SemanticKernelAlertGenerator : IAlertMessageGenerator
    {
        private readonly Kernel _kernel;

        public SemanticKernelAlertGenerator(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<(string message, string recommendation)> GenerateAsync(string kpiName, decimal currentValue, decimal threshold, string historyContext)
        {
            var prompt = $@"You are a business analyst.
                            KPI: {kpiName}
                            Current value: {currentValue}
                            Expected (threshold): {threshold}
                            Historical context: {historyContext}
                            Write ONE short alert message (max 20 words).
                            Then write ONE short recommendation (max 25 words).
                            Return JSON: {{""message"": ""..."", ""recommendation"": ""...""}}";

            var result = await _kernel.InvokePromptAsync(prompt);

            // Strip markdown code fences that LLMs sometimes wrap around JSON output
            var json = result.ToString().Trim();
            json = Regex.Replace(json, @"^```[a-zA-Z]*\s*", "").TrimStart();
            json = Regex.Replace(json, @"```\s*$", "").TrimEnd();

            var parsed = JsonSerializer.Deserialize<AlertJsonResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (parsed?.message ?? "Anomaly detected.", parsed?.recommendation ?? "Please review the KPI.");
        }

        private record AlertJsonResult(string message, string recommendation);
    }
}
