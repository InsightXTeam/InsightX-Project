namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Retrieved chunk with its source metadata and similarity score.
    /// </summary>
    public class RetrievedChunkDto
    {
        public string Text { get; set; } = string.Empty;

        public int ReportId { get; set; }

        public int DepartmentId { get; set; }

        public string Month { get; set; } = string.Empty;

        public int Year { get; set; }

        // Similarity score returned by the vector search.
        public float Score { get; set; }
    }
}
