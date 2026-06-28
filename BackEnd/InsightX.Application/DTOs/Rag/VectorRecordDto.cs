namespace Insight_test.All.Dto
{
    /// <summary>
    /// Vector record stored in the vector database.
    /// </summary>
    public class VectorRecordDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Vector embedding representation.
        public float[] Vector { get; set; } = Array.Empty<float>();

        public string Text { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public int DepartmentId { get; set; }

        public int ReportId { get; set; }

        public string Month { get; set; } = string.Empty;

        public int Year { get; set; }
    }
}
