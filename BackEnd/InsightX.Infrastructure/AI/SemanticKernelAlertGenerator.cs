using InsightX.Application.Interfaces;
using Microsoft.SemanticKernel;
using System.Text.Json;

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

            var json = result.ToString();
            var parsed = JsonSerializer.Deserialize<(string message, string recommendation)>(json);

            return parsed;
        }
    }
}
