using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace InsightX.Infrastructure.AI.Rag;

public static class QdrantInitializer
{
    private const string CollectionName = "reports";

    public static async Task InitializeAsync(QdrantClient qdrant)
    {
        var collections = await qdrant.ListCollectionsAsync();

        if (!collections.Contains(CollectionName))
        {
            await qdrant.CreateCollectionAsync(
                collectionName: CollectionName,
                vectorsConfig: new VectorParams
                {
                    // this sizw in nvidia/llama model case
                    Size = 1536,
                    Distance = Distance.Cosine
                });
        }
    }
}