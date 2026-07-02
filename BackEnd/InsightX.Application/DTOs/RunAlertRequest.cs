namespace InsightX.Application.DTOs
{
    public class RunAlertRequest
    {
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public string KpiName { get; set; }
        public decimal CurrentValue { get; set; }
    }
}
