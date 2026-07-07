using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace InsightX.Infrastructure.AI.Agent
{
    public class ItiChatCompletionService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _chatApiUrl;
        private readonly string _apiKey;
        private readonly string _modelId;

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public ItiChatCompletionService(HttpClient httpClient, string chatApiUrl, string apiKey, string modelId)
        {
            _httpClient = httpClient;
            _chatApiUrl = chatApiUrl;
            _apiKey = apiKey;
            _modelId = modelId;
        }

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            CancellationToken cancellationToken = default)
        {
            var systemMessage = chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System)?.Content ?? "";
            
            var messages = chatHistory
                .Where(m => m.Role != AuthorRole.System)
                .Select(m => new Message(
                    Role: m.Role == AuthorRole.User ? "user" : "assistant",
                    Content: m.Content ?? ""
                )).ToArray();

            var request = new ChatCompletionRequest(
                ModelId: _modelId,
                SystemPrompt: systemMessage,
                Messages: messages,
                MaxTokens: 2000
            );

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _chatApiUrl)
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);
            if (result == null || string.IsNullOrEmpty(result.OutputText))
            {
                throw new InvalidOperationException("Invalid AI response.");
            }

            return new[] { new ChatMessageContent(AuthorRole.Assistant, result.OutputText) };
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Fallback to non-streaming for now
            var contents = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
            foreach (var content in contents)
            {
                yield return new StreamingChatMessageContent(content.Role, content.Content);
            }
        }
        
        private sealed record ChatCompletionRequest(
            [property: JsonPropertyName("model_id")] string ModelId,
            [property: JsonPropertyName("system_prompt")] string SystemPrompt,
            [property: JsonPropertyName("messages")] Message[] Messages,
            [property: JsonPropertyName("max_tokens")] int MaxTokens);

        private sealed record Message(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("content")] string Content);

        private sealed record ChatCompletionResponse(
            [property: JsonPropertyName("output_text")] string OutputText);
    }
}
