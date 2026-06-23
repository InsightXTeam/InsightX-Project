namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Request model for retrieving relevant chunks.
    /// </summary>
    public class RetrieveRequestDto
    {

        public string Question { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public int TopK { get; set; } = 5;
    }
}
