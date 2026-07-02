using InsightX.Application.Interfaces;
using InsightX.Domain.Enums;
using InsightX.Domain.ValueObjects;

namespace InsightX.Infrastructure.Anomaly.Rules
{
    public class ThresholdRule : IAnomalyRule
    {
        private readonly IMetricsRepository _metricsRepository;

        public ThresholdRule(IMetricsRepository metricsRepository)
        {
            _metricsRepository = metricsRepository;
        }

        public async Task<AnomalyResult> CheckAsync(int companyId, string kpiName, decimal currentValue)
        {
            var config = await _metricsRepository.GetKPIConfigAsync(companyId, kpiName);
            if (config == null) return AnomalyResult.None();

            if (currentValue < config.Threshold)
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
