namespace InsightX.Domain.Entities.Auth
{
    public class KPI
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Threshold { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}
