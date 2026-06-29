using Insight_test.All.Dto;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Insight_test.All.Services
{
    /// <summary>
    /// Qdrant implementation of vector storage and retrieval operations.
    /// </summary>
    public class QdrantVectorStore : IVectorStore
    {
        private const string CollectionName = "reports";

        private readonly QdrantClient _client;

        // QdrantClient is created once and injected as a singleton
        // (registered in Program.cs, Phase 8) so we reuse one connection.
        public QdrantVectorStore(QdrantClient client)
        {
            _client = client;
        }

        public async Task UpsertAsync(List<VectorRecordDto> records, CancellationToken cancellationToken = default)
        {
            if (records == null || records.Count == 0)
                return;

            // Map application records to Qdrant points.
            var points = records.Select(record => new PointStruct
            {
                Id = new PointId { Uuid = record.Id },
                Vectors = record.Vector,
                Payload =
                {
                    ["text"] = record.Text,
                    ["company_id"] = record.CompanyId,
                    ["department_id"] = record.DepartmentId,
                    ["report_id"] = record.ReportId,
                    ["month"] = record.Month,
                    ["year"] = record.Year
                }
            }).ToList();

            await _client.UpsertAsync(CollectionName, points, cancellationToken: cancellationToken);
        }

        public async Task<List<RetrievedChunkDto>> SearchAsync(
            float[] queryVector,
            int companyId,
            int topK,
            CancellationToken cancellationToken = default)
        {
            // Restrict search results to the requested company.
            // Company-level isolation is enforced on every search.
            var companyFilter = new Filter
            {
                Must = { Conditions.Match("company_id", companyId) }
            };

            var results = await _client.SearchAsync(
                collectionName: CollectionName,
                vector: queryVector,
                filter: companyFilter,
                limit: (ulong)topK,
                cancellationToken: cancellationToken);

            // Convert Qdrant's ScoredPoint results back into our own DTO.
            return results.Select(point => new RetrievedChunkDto
            {
                Text = point.Payload["text"].StringValue,
                ReportId = (int)point.Payload["report_id"].IntegerValue,
                DepartmentId = (int)point.Payload["department_id"].IntegerValue,
                Month = point.Payload["month"].StringValue,
                Year = (int)point.Payload["year"].IntegerValue,
                Score = point.Score
            }).ToList();
        }

        public async Task DeleteByReportIdAsync(int companyId, int reportId, CancellationToken cancellationToken = default)
        {
            // Delete every point whose "company_id & report_id" payload field matches.
            var reportFilter = new Filter
            {
                Must = {
                    Conditions.Match("company_id", companyId),
                    Conditions.Match("report_id", reportId)
                }
            };

            await _client.DeleteAsync(CollectionName, reportFilter, cancellationToken: cancellationToken);
        }
    }
}
