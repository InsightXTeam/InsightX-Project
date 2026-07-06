using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsightX.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InsightX.Infrastructure.AI
{
    public class OllamaAlertGenerator : IAlertMessageGenerator
    {
        private readonly HttpClient _httpClient;
        private readonly string _chatApiUrl;
        private readonly string _apiKey;
        private readonly string _model;

        public OllamaAlertGenerator(HttpClient httpClient, IConfiguration configuration)
        {
            var alertAiConfig = configuration.GetSection("AlertAI");
            _httpClient = httpClient;
            _chatApiUrl = alertAiConfig["ChatEndpoint"] ?? throw new ArgumentNullException("AlertAI:ChatEndpoint is required");
            _apiKey = alertAiConfig["ApiKey"] ?? throw new ArgumentNullException("AlertAI:ApiKey is required");
            _model = alertAiConfig["ModelId"] ?? throw new ArgumentNullException("AlertAI:ModelId is required");
        }

        public async Task<(string message, string recommendation)> GenerateAsync(string kpiName, decimal currentValue, decimal threshold, string historyContext)
        {
            var prompt = $@"
You are a business analyst.
KPI: {kpiName}
Current value: {currentValue}
Expected (threshold): {threshold}
Historical context: {historyContext}
Write ONE short alert message (max 20 words).
Then write ONE short recommendation (max 25 words).
Return ONLY a valid JSON object in this format: {{""message"": ""..."", ""recommendation"": ""...""}}
Do NOT include markdown backticks or any other text.
";

            var request = new ChatCompletionRequest(
                ModelId: _model,
                SystemPrompt: "You are an assistant that generates short business alerts in JSON format.",
                Messages: new[] { new Message(Role: "user", Content: prompt) },
                MaxTokens: 200);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _chatApiUrl)
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return ("Anomaly detected.", "Please review the KPI.");
            }

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
            if (result == null || string.IsNullOrEmpty(result.OutputText))
            {
                return ("Anomaly detected.", "Please review the KPI.");
            }

            var json = result.OutputText.Trim();
            json = System.Text.RegularExpressions.Regex.Replace(json, @"^```[a-zA-Z]*\s*", "").TrimStart();
            json = System.Text.RegularExpressions.Regex.Replace(json, @"```\s*$", "").TrimEnd();

            AlertJsonResult? parsed = null;
            try
            {
                parsed = JsonSerializer.Deserialize<AlertJsonResult>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                // Fallback
            }

            return (parsed?.message ?? "Anomaly detected.", parsed?.recommendation ?? "Please review the KPI.");
        }

        private record AlertJsonResult(string message, string recommendation);

        private sealed record ChatCompletionRequest(
            [property: JsonPropertyName("model_id")] string ModelId,
            [property: JsonPropertyName("system_prompt")] string SystemPrompt,
            [property: JsonPropertyName("messages")] Message[] Messages,
            [property: JsonPropertyName("max_tokens")] int MaxTokens = 1000);

        private sealed record Message(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content")] string Content);

        private sealed record ChatCompletionResponse(
            [property: JsonPropertyName("output_text")] string OutputText);
    }
}
