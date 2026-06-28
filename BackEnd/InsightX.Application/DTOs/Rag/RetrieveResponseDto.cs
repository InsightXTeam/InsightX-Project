namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Retrieval result containing matching chunks.
    /// </summary>
    public class RetrieveResponseDto
    {
        // Retrieved chunks
        public List<RetrievedChunkDto> Chunks { get; set; } = new();
    }
}
