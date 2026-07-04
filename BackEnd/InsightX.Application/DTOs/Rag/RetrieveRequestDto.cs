using System.ComponentModel;

namespace InsightXAI.Application.DTOs
{
    /// <summary>
    /// Request model for retrieving relevant chunks.
    /// </summary>
    public class RetrieveRequestDto
    {
        public string Question { get; set; } = string.Empty;

        [DefaultValue(1)]
        public int TopK { get; set; } = 1;
    }
}
