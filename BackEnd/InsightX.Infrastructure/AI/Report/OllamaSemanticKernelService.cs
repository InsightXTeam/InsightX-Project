using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InsightX.Infrastructure.AI.Report
{
    public class OllamaSemanticKernelService : IAIExtractionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _chatApiUrl;
        private readonly string _imageApiUrl;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _imageModel;

        public OllamaSemanticKernelService(HttpClient httpClient, IConfiguration configuration)
        {
            var reportAiConfig = configuration.GetSection("ReportAI");
            _httpClient = httpClient;
            _chatApiUrl = reportAiConfig["ChatEndpoint"] ?? configuration["ITI_API_URL"] ?? configuration["AI_API_URL"] ?? throw new ArgumentNullException("ReportAI:ChatEndpoint is required");
            _imageApiUrl = reportAiConfig["ImagesEndpoint"] ?? configuration["ITI_IMAGE_API_URL"] ?? _chatApiUrl;
            _apiKey = reportAiConfig["ApiKey"] ?? configuration["ITI_API_KEY"] ?? configuration["AI_API_KEY"] ?? throw new ArgumentNullException("ReportAI:ApiKey is required");
            _model = reportAiConfig["ModelId"] ?? configuration["ITI_MODEL_ID"] ?? configuration["AI_MODEL"] ?? throw new ArgumentNullException("ReportAI:ModelId is required");
            _imageModel = reportAiConfig["ModelId"] ?? configuration["ITI_IMAGE_MODEL_ID"] ?? _model;
        }

        public async Task<string> ExtractMetricsAsync(string text, IEnumerable<KpiResponseDto> predefinedKPIs, string? departmentName = null)
        {
            var kpiList = predefinedKPIs.ToList();
            var kpiListStr = string.Join("\n", kpiList.Select((k, i) =>
            {
                var deptStr = !string.IsNullOrWhiteSpace(k.DepartmentName) ? $" [Department: {k.DepartmentName}]" : " [Company-wide / General]";
                var descStr = !string.IsNullOrWhiteSpace(k.Description) ? $" - Description/Meaning: {k.Description}" : "";
                return $"  {i + 1}. \"{k.Name}\"{deptStr}{descStr}";
            }));

            var deptContextStr = !string.IsNullOrWhiteSpace(departmentName)
                ? $"This report is specifically associated with the \"{departmentName}\" department. Pay extra attention to metrics and tables relevant to this department.\n\n"
                : "";

            // Truncate text if too long to avoid exceeding model context window
            const int maxTextLength = 12000;
            var documentText = text;
            if (documentText.Length > maxTextLength)
            {
                documentText = documentText.Substring(0, maxTextLength) + "\n\n[... Document truncated for processing ...]";
            }

            var prompt = $@"You are a financial data analyst. Your task is to understand the report well and extract exact KPI (Key Performance Indicator) values from the document below.

## Step-by-step instructions:

### Step 1: Identify all numeric values in the document
Scan the entire document for any numeric data — including values in tables, lists, paragraphs, headers, and footers.
Pay special attention to:
- Tables with headers and rows of data
- Lines formatted as ""Label: Value"" or ""Label = Value""
- Percentages (e.g., ""85%"" → 85)
- Currency values (e.g., ""$1.2M"" → 1200000, ""$500K"" → 500000, ""€3,450"" → 3450)
- Abbreviated numbers (e.g., ""1.5M"" → 1500000, ""200K"" → 200000)
- Negative values (e.g., ""-5%"" → -5, ""($200)"" → -200)

### Step 2: Match each numeric value to one of these predefined KPIs
Use FUZZY matching — a document may use different wording than the exact KPI name.
Carefully analyze the context of the report (headers, tables, summaries, footnotes, and department names) to understand what each numeric value represents.
{deptContextStr}For example:
- ""Total Revenue"" or ""Net Revenue"" matches a KPI named ""Revenue""
- ""Customer Satisfaction Score"" matches ""Customer Satisfaction""
- ""Employee Turnover Rate"" matches ""Employee Turnover""
- ""Monthly Sales"" matches ""Sales""

The predefined KPIs to look for are:
{kpiListStr}

IMPORTANT: Only extract KPIs from the list above. Do NOT invent KPIs that are not in this list.

### Step 3: Determine the time period (Month and Year)
Look for dates in:
- Document headers, titles, or footers (e.g., ""Q3 2025 Report"", ""March 2025"")
- Table column headers (e.g., ""Jan"", ""Feb"", ""2025"")
- Contextual clues in the text (e.g., ""For the month of June 2025"")

Quarter mappings: Q1=3, Q2=6, Q3=9, Q4=12 (use the last month of the quarter).

### Step 4: Return ONLY a valid JSON array

CRITICAL: Your entire response must be ONLY a valid JSON array. No explanation, no markdown, no text before or after.

Format:
[
  {{
    ""KPIName"": ""<exact name from the predefined list>"",
    ""Value"": <numeric value as a number, NOT a string>,
    ""Month"": <1-12 or null if unknown>,
    ""Year"": <4-digit year or null if unknown>
  }}
]

If a KPI appears multiple times or for different months, extract ONLY ONE single entry representing the most relevant or latest value in the document. Do not return duplicate entries for the same KPI.
If no matching KPIs are found, return an empty array: []

## Document to analyze:
{documentText}";

            var request = new ChatCompletionRequest(
                ModelId: _model,
                SystemPrompt: "You are an expert financial data extraction system. You understand reports well and extract structured KPI data from documents with high precision. You always respond with valid JSON only — never with explanations or markdown formatting.",
                Messages: new[] { new Message(Role: "user", Content: prompt) },
                MaxTokens: 4000);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _chatApiUrl)
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
            if (result == null || string.IsNullOrEmpty(result.OutputText))
            {
                throw new InvalidOperationException("AI response did not contain output_text.");
            }

            return result.OutputText;
        }

        public async Task<string> ExtractTextFromImageAsync(byte[] imageBytes, string fileName)
        {
            var prompt = "Extract all readable text from the image and return only the text content without any commentary or formatting.";
            var base64 = Convert.ToBase64String(imageBytes);
            var format = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            if (format == "jpg") format = "jpeg";

            var request = new ChatCompletionRequest(
                ModelId: _imageModel,
                Messages: new[] { 
                    new Message(
                        Role: "user", 
                        Text: prompt,
                        Images: new[] { new ImagePayload(Format: format, DataBase64: base64) }
                    ) 
                },
                MaxTokens: 2000);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _imageApiUrl)
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
            if (result == null || string.IsNullOrEmpty(result.OutputText))
            {
                throw new InvalidOperationException("AI response did not contain output_text.");
            }

            return result.OutputText.Trim();
        }

        private static string GetMimeType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        private sealed record ChatCompletionRequest(
            [property: JsonPropertyName("model_id")] string ModelId,
            [property: JsonPropertyName("messages")] Message[] Messages,
            [property: JsonPropertyName("system_prompt"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SystemPrompt = null,
            [property: JsonPropertyName("max_tokens")] int MaxTokens = 1000);

        private sealed record ImagePayload(
            [property: JsonPropertyName("format")] string Format,
            [property: JsonPropertyName("data_base64")] string DataBase64);

        private sealed record Message(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Content = null,
            [property: JsonPropertyName("text"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text = null,
            [property: JsonPropertyName("images"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ImagePayload[]? Images = null);

        private sealed record ChatCompletionResponse(
            [property: JsonPropertyName("output_text")] string OutputText);
    }
}