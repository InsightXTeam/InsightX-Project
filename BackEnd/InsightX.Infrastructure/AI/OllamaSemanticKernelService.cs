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
2. Month MUST be a number between 1 and 12 based on the extracted date. If no month is found, use 0.
3. Year MUST be a 4-digit number based on the extracted date. If no year is found, use 0.
4. Value MUST be the actual numeric value extracted from the document. Do not use example values.
5. Do not invent values. Only return data that exists in the document.

Output Format Example:
[
  {{
    ""KPIName"": ""<One of the predefined KPIs>"",
    ""Value"": <extracted numeric value>,
    ""Month"": <extracted month number>,
    ""Year"": <extracted 4-digit year>
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