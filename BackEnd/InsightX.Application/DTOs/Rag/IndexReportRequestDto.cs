namespace InsightX.Application.DTOs.Rag
{
    public class IndexReportRequestDto
    {
        public string Text { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public int ReportId { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
    }
}
