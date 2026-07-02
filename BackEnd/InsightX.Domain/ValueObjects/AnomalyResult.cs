using InsightX.Domain.Enums;

namespace InsightX.Domain.ValueObjects
{
    public class AnomalyResult
    {
        public bool IsAnomaly { get; set; }
        public AnomalyLevel Level { get; set; }
        public string KPIName { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Threshold { get; set; }

        public static AnomalyResult None()
        {
            return new AnomalyResult()
            {
                IsAnomaly = false,
                Level = AnomalyLevel.None
            };
        }
    }
}
