namespace InsightX.Application.DTOs.Rag
{
    public class SearchChunkDto
    {
        public string Text { get; set; }

        public double Score { get; set; }

        public int ReportId { get; set; }
    }
}
