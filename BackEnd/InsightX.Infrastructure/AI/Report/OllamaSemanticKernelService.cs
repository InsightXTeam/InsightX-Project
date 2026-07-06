using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
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

            var request = new ChatCompletionRequest(
                ModelId: _model,
                SystemPrompt: "You are an assistant that extracts structured KPI JSON from documents.",
                Messages: new[] { new Message(Role: "user", Content: prompt) },
                MaxTokens: 1000);

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
            var mimeType = GetMimeType(fileName);
            var base64 = Convert.ToBase64String(imageBytes);
            var imageDataUri = $"data:{mimeType};base64,{base64}";

            var request = new ChatCompletionRequest(
                ModelId: _imageModel,
                SystemPrompt: "You are an assistant that extracts text from images.",
                Messages: new[] { new Message(Role: "user", Content: prompt) },
                InputImage: new[] { new ImageInput(Image: imageDataUri) },
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
            [property: JsonPropertyName("system_prompt")] string SystemPrompt,
            [property: JsonPropertyName("messages")] Message[] Messages,
            [property: JsonPropertyName("input_image")] ImageInput[]? InputImage = null,
            [property: JsonPropertyName("max_tokens")] int MaxTokens = 1000);

        private sealed record ImageInput(
            [property: JsonPropertyName("image")] string Image);

        private sealed record Message(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content")] string Content);

        private sealed record ChatCompletionResponse(
            [property: JsonPropertyName("output_text")] string OutputText);
    }
}