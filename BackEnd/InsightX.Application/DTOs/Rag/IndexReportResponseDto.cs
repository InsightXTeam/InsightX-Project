namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Result of a successful report indexing operation.
    /// </summary>
    public class IndexReportResponseDto
    {
        // Indexed report identifier.
        public int ReportId { get; set; }

        // Number of chunks indexed for the report.
        public int ChunksIndexed { get; set; }
    }
}
