using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Insight_test.All.Initialization;

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
                    Size = 1536,
                    Distance = Distance.Cosine
                });
        }
    }
}