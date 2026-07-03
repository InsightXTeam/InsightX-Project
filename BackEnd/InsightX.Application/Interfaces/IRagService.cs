using System.Collections.Generic;
using System.Threading.Tasks;

namespace InsightX.Application.Interfaces
{
    public class RagChunk
    {
        public string Text { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public int ReportId { get; set; }
        public int DepartmentId { get; set; }
    }

    public interface IRagService
    {
        Task<List<RagChunk>> RetrieveRelevantChunksAsync(string question, int companyId, int limit = 5);
    }
}
