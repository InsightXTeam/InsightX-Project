namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Request model used to index a report into the vector store.
    /// </summary>
    public class IndexReportRequestDto
    {
        public int ReportId { get; set; }

        public int CompanyId { get; set; }

        public int DepartmentId { get; set; }

        public string Month { get; set; } = string.Empty;

        public int Year { get; set; }

        public string FullText { get; set; } = string.Empty;
    }
}
