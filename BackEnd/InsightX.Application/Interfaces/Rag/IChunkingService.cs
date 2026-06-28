using InsightXAI.Application.DTOs;

namespace InsightXAI.Application.Interfaces
{
    /// <summary>
    /// Provides text chunking functionality for report content.
    /// </summary>
    public interface IChunkingService
    {
        // Splits report text into smaller chunks.
        List<ChunkDto> SplitIntoChunks(string fullText);
    }
}
