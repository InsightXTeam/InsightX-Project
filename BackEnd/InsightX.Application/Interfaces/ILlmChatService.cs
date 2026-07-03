using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InsightX.Application.Interfaces
{
    public interface ILlmChatService
    {
        Task<string> GenerateAnswerAsync(
            string question,
            IReadOnlyList<RagChunk> chunks,
            CancellationToken cancellationToken = default);
    }
}
