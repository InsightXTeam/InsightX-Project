using System.Net.Http.Json;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InsightX.Infrastructure.AI
{
    public class RagService : IRagService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RagService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IRagRetrievalService _ragRetrievalService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RagService(
            IConfiguration configuration,
            ILogger<RagService> logger,
            HttpClient httpClient,
            IRagRetrievalService ragRetrievalService,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _ragRetrievalService = ragRetrievalService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<RagChunk>> RetrieveRelevantChunksAsync(string question, int companyId, int limit = 5)
        {
            var ragApiUrl = _configuration["RagService:BaseUrl"];

            if (!string.IsNullOrEmpty(ragApiUrl))
            {
                var apiChunks = await TryRetrieveFromPerson4ApiAsync(ragApiUrl, question, limit);
                if (apiChunks.Count > 0)
                {
                    return apiChunks;
                }

                _logger.LogWarning("Person 4 RAG API returned no results. Falling back to local retrieval.");
            }

            return await _ragRetrievalService.RetrieveAsync(question, companyId, limit);
        }

        private async Task<List<RagChunk>> TryRetrieveFromPerson4ApiAsync(
            string baseUrl,
            string question,
            int limit)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/rag/retrieve")
                {
                    Content = JsonContent.Create(new { question, limit })
                };

                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", authHeader);
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Person 4 RAG API returned status {StatusCode}.", response.StatusCode);
                    return new List<RagChunk>();
                }

                var chunks = await response.Content.ReadFromJsonAsync<List<RagChunk>>();
                return chunks ?? new List<RagChunk>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call Person 4 RAG API.");
                return new List<RagChunk>();
            }
        }
    }
}
