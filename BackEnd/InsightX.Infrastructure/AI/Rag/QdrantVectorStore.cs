using InsightX.Infrastructure.AI.Rag;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace InsightX.Infrastructure.AI.Rag
{
    public class QdrantVectorStore : IVectorStore
    {
        private readonly QdrantClient _qdrantClient;
        private readonly string _collectionName;

        public QdrantVectorStore(QdrantClient qdrantClient, IConfiguration configuration)
        {
            _qdrantClient = qdrantClient;
            _collectionName = configuration["Qdrant:CollectionName"] ?? "reports";
        }

        public async Task UpsertAsync(List<VectorRecordDto> records, CancellationToken cancellationToken = default)
        {
            var points = new List<PointStruct>();
            foreach (var record in records)
            {
                var point = new PointStruct
                {
                    Id = Guid.NewGuid(),
                    Vectors = record.Vector.ToArray(),
                    Payload =
                    {
                        ["companyId"] = record.CompanyId,
                        ["departmentId"] = record.DepartmentId,
                        ["reportId"] = record.ReportId,
                        ["text"] = record.Text,
                        ["month"] = record.Month,
                        ["year"] = record.Year
                    }
                };
                points.Add(point);
            }

            await _qdrantClient.UpsertAsync(_collectionName, points, cancellationToken: cancellationToken);
        }

        public async Task<List<RetrievedChunkDto>> SearchAsync(float[] queryVector, int companyId, int topK, int? departmentId = null, CancellationToken cancellationToken = default)
        {
            var filters = new List<Condition>
            {
                Conditions.Match("companyId", companyId)
            };

            if (departmentId.HasValue)
            {
                filters.Add(Conditions.Match("departmentId", departmentId.Value));
            }

            var filter = new Filter
            {
                Must = { filters }
            };

            var searchResult = await _qdrantClient.SearchAsync(
                collectionName: _collectionName,
                vector: queryVector.ToArray(),
                filter: filter,
                limit: (ulong)topK,
                cancellationToken: cancellationToken);

            var chunks = new List<RetrievedChunkDto>();
            foreach (var point in searchResult)
            {
                chunks.Add(new RetrievedChunkDto
                {
                    Text = point.Payload["text"].StringValue,
                    Score = point.Score,
                    ReportId = (int)point.Payload["reportId"].IntegerValue,
                    Month = point.Payload["month"].StringValue,
                    Year = (int)point.Payload["year"].IntegerValue
                });
            }

            return chunks;
        }

        public async Task DeleteByReportIdAsync(int companyId, int reportId, CancellationToken cancellationToken = default)
        {
            var filter = new Filter
            {
                Must =
                {
                    Conditions.Match("companyId", companyId),
                    Conditions.Match("reportId", reportId)
                }
            };

            await _qdrantClient.DeleteAsync(_collectionName, filter, cancellationToken: cancellationToken);
        }
    }
}
