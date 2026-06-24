using InsightX.Application.Interfaces;
using Microsoft.SemanticKernel;

namespace InsightX.Infrastructure.AI
{
    public class OllamaSemanticKernelService : IAIExtractionService
    {
        private readonly Kernel _kernel;

        public OllamaSemanticKernelService()
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(modelId: "qwen2.5:1.5b", apiKey: "ollama", endpoint: new Uri("http://localhost:11434/v1"));

            _kernel = builder.Build();
        }

        public async Task<string> ExtractMetricsAsync(string text)
        {
            var prompt = $@"

Extract metrics.

Return ONLY JSON.

Rules:

-Month must be integer
- Year must be integer
- Value must be number

Format:

[
 {{
  ""KPIName"": ""Revenue"",
  ""Value"": 50000,
  ""Month"": 6,
  ""Year"": 2026
 }}
]
Document:

            {text}

            ";

            var result = await _kernel.InvokePromptAsync(prompt);

            return result.ToString();
        }
    }
}