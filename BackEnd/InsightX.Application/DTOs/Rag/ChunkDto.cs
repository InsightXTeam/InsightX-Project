namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Report text chunk before vectorization.
    /// </summary>
    public class ChunkDto
    {
        // Chunk content.
        public string Text { get; set; } = string.Empty;

        // Chunk order in the original report.
        public int ChunkIndex { get; set; }
    }
}
