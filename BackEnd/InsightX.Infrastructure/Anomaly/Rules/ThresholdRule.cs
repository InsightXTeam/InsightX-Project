using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Enums;
using InsightX.Domain.ValueObjects;

namespace InsightX.Infrastructure.Anomaly.Rules
{
    public class ThresholdRule : IAnomalyRule
    {
        public async Task<AnomalyResult> CheckAsync(int companyId, string kpiName, decimal currentValue, KPIConfig config)
        {
            var breached = config.ThresholdDirection == ThresholdDirection.Below
                ? currentValue < config.Threshold
                : currentValue > config.Threshold;

            if (breached)
                return new AnomalyResult
                {
                    IsAnomaly = true,
                    Level = AnomalyLevel.Threshold,
                    KPIName = kpiName,
                    CurrentValue = currentValue,
                    Threshold = config.Threshold
                };

            return AnomalyResult.None();
        }
    }
}
