// TEMP: will be replaced by Person 2's implementation
namespace InsightX.Domain.Entities
{
    public class HistoricalMetric
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public string KPIName { get; set; }
        public decimal Value { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
