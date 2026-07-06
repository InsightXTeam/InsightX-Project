using InsightX.Domain.Enums;

namespace InsightX.Application.DTOs
{
    public class AlertDto
    {
        public int Id { get; set; }
        public string KPIName { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Threshold { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
        public bool SeenByOwner { get; set; }
        public DateTime CreatedAt { get; set; }
        public AlertType AlertType { get; set; }
    }
}
