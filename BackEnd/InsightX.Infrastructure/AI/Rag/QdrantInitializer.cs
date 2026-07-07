using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace InsightX.Infrastructure.AI.Rag;

public static class QdrantInitializer
{
    private const string CollectionName = "reports";

    public static async Task InitializeAsync(QdrantClient qdrant, IConfiguration configuration)
    {
        var collections = await qdrant.ListCollectionsAsync();

        if (!collections.Contains(CollectionName))
        {
            var vectorSize = configuration.GetValue<ulong>("Qdrant:Size");

            await qdrant.CreateCollectionAsync(
                collectionName: CollectionName,
                vectorsConfig: new VectorParams
                {
                    Size = vectorSize,
                    Distance = Distance.Cosine
                });
        }

        // Always ensure payload indexes exist (idempotent)
        await qdrant.CreatePayloadIndexAsync(
            CollectionName,
            "companyId", PayloadSchemaType.Integer);

        await qdrant.CreatePayloadIndexAsync(
            CollectionName,
            "departmentId", PayloadSchemaType.Integer);

        await qdrant.CreatePayloadIndexAsync(
            CollectionName,
            "reportId", PayloadSchemaType.Integer);
    }
}