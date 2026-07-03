namespace InsightX.Application.DTOs
{
    public class RagRetrieveRequestDto
    {
        public string Question { get; set; } = string.Empty;
        public int Limit { get; set; } = 5;
    }
}
