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

        public async Task<string> ExtractMetricsAsync(string text, IEnumerable<string> predefinedKPIs)
        {
            var kpiListStr = string.Join(", ", predefinedKPIs);

            var prompt = $@"
Analyze the document and extract the specified KPIs.
Return ONLY a valid JSON array. Do not include any text outside the JSON.

Rules:
1. ONLY extract these KPIs: {kpiListStr}
2. Month MUST be a number between 1 and 12 (e.g. 6 for June). If no month is found, use 0.
3. Year MUST be a 4-digit number (e.g. 2024). If no year is found, use 0.
4. Value MUST be a numeric value (e.g. 50000).

Output Format Example:
[
  {{
    ""KPIName"": ""<One of the predefined KPIs>"",
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