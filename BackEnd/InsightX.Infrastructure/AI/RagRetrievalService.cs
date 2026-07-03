using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using InsightX.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InsightX.Infrastructure.AI
{
    public class RagRetrievalService : IRagRetrievalService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RagRetrievalService> _logger;
        private readonly IEmbeddingService _embeddingService;
        private readonly HttpClient _httpClient;

        private static readonly List<RagChunk> FallbackChunks = new()
        {
            new RagChunk
            {
                Text = "March 2026 Production Report for InsightX Manufacturing Corp. Production was 600 units, which failed to reach the threshold of 1000 units. The drop was caused by a critical conveyor belt breakdown on March 10 that halted operations for 5 days. Revenue was $7,000 against a threshold of $10,000 due to lower production output.",
                Month = "March",
                Year = 2026,
                ReportId = 3,
                DepartmentId = 1
            },
            new RagChunk
            {
                Text = "March 2026 Quality Control Report. Defect rate spiked to 7% in March, which exceeded the acceptable threshold of 5%. This was due to temporary calibration issues on the backup assembly line which was activated during the conveyor belt failure.",
                Month = "March",
                Year = 2026,
                ReportId = 3,
                DepartmentId = 2
            },
            new RagChunk
            {
                Text = "March 2026 Human Resources Report. Absent employees rose to 8 in March, crossing the threshold of 5. This was due to a local seasonal flu outbreak affecting the assembly floor staff.",
                Month = "March",
                Year = 2026,
                ReportId = 3,
                DepartmentId = 3
            },
            new RagChunk
            {
                Text = "January 2026 Operations Report. Production reached 1050 units (threshold 1000). Defect rate was 3.0% (threshold 5%). Absent employees was 2 (threshold 5). Revenue was $12,000 (threshold $10,000). All metrics were within normal operating parameters. Conveyor belt was fully operational.",
                Month = "January",
                Year = 2026,
                ReportId = 1,
                DepartmentId = 1
            },
            new RagChunk
            {
                Text = "February 2026 Operations Report. Production reached 1100 units (threshold 1000). Defect rate was 2.5% (threshold 5%). Absent employees was 3 (threshold 5). Revenue was $13,000 (threshold $10,000). Operations were optimal with high efficiency across all lines.",
                Month = "February",
                Year = 2026,
                ReportId = 2,
                DepartmentId = 1
            }
        };

        public RagRetrievalService(
            IConfiguration configuration,
            ILogger<RagRetrievalService> logger,
            IEmbeddingService embeddingService,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _embeddingService = embeddingService;
            _httpClient = httpClient;
        }

        public async Task<List<RagChunk>> RetrieveAsync(string question, int companyId, int limit = 5)
        {
            var qdrantUrl = _configuration["Qdrant:Url"];
            if (!string.IsNullOrEmpty(qdrantUrl))
            {
                var qdrantChunks = await TryRetrieveFromQdrantAsync(qdrantUrl, question, companyId, limit);
                if (qdrantChunks.Count > 0)
                {
                    return qdrantChunks;
                }
            }

            _logger.LogInformation("Using local fallback RAG search for company {CompanyId}.", companyId);
            return RetrieveFallback(question, limit);
        }

        private async Task<List<RagChunk>> TryRetrieveFromQdrantAsync(
            string qdrantUrl,
            string question,
            int companyId,
            int limit)
        {
            try
            {
                var vector = await _embeddingService.GenerateEmbeddingAsync(question);
                if (vector == null)
                {
                    return new List<RagChunk>();
                }

                var requestBody = new
                {
                    vector,
                    filter = new
                    {
                        must = new[]
                        {
                            new { key = "company_id", match = new { value = companyId } }
                        }
                    },
                    limit,
                    with_payload = true
                };

                _httpClient.BaseAddress = new Uri(qdrantUrl);
                var response = await _httpClient.PostAsJsonAsync("/collections/reports/points/search", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<RagChunk>();
                }

                var jsonResult = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                var chunks = new List<RagChunk>();

                if (jsonResult.TryGetProperty("result", out var results))
                {
                    foreach (var point in results.EnumerateArray())
                    {
                        if (point.TryGetProperty("payload", out var payload))
                        {
                            chunks.Add(new RagChunk
                            {
                                Text = payload.GetProperty("text").GetString() ?? string.Empty,
                                Month = payload.TryGetProperty("month", out var m)
                                    ? m.GetString() ?? string.Empty
                                    : string.Empty,
                                Year = payload.TryGetProperty("year", out var y)
                                    ? y.ValueKind == System.Text.Json.JsonValueKind.Number ? y.GetInt32() : 2026
                                    : 2026,
                                ReportId = payload.TryGetProperty("report_id", out var r) ? r.GetInt32() : 0,
                                DepartmentId = payload.TryGetProperty("department_id", out var d) ? d.GetInt32() : 0
                            });
                        }
                    }
                }

                return chunks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve chunks from Qdrant.");
                return new List<RagChunk>();
            }
        }

        private static List<RagChunk> RetrieveFallback(string question, int limit)
        {
            if (string.IsNullOrEmpty(question))
            {
                return FallbackChunks.Take(limit).ToList();
            }

            var terms = question.ToLowerInvariant()
                .Split(new[] { ' ', '?', ',', '.', '!' }, StringSplitOptions.RemoveEmptyEntries);

            var scored = FallbackChunks.Select(chunk =>
            {
                var textLower = chunk.Text.ToLowerInvariant();
                var score = 0;

                foreach (var term in terms)
                {
                    if (term.Length <= 2 || !textLower.Contains(term))
                    {
                        continue;
                    }

                    score += 2;
                    if (term == "march" && chunk.Month == "March") score += 5;
                    if (term == "january" && chunk.Month == "January") score += 5;
                    if (term == "february" && chunk.Month == "February") score += 5;
                    if (term is "drop" or "anomaly" or "conveyor" or "breakdown" && chunk.Month == "March") score += 3;
                }

                return new { Chunk = chunk, Score = score };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Chunk)
            .ToList();

            return scored.Count > 0
                ? scored.Take(limit).ToList()
                : FallbackChunks.Take(limit).ToList();
        }
    }
}
