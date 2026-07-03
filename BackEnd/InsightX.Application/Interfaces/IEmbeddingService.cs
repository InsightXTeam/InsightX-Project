using System.Threading;
using System.Threading.Tasks;

namespace InsightX.Application.Interfaces
{
    public interface IEmbeddingService
    {
        bool IsConfigured { get; }
        Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}
