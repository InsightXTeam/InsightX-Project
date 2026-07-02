// TEMP: will be replaced by Person 1's implementation
namespace InsightX.Domain.Entities
{
    public class KPI
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public decimal Threshold { get; set; }
    }
}
