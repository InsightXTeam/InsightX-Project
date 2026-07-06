using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Enums;
using InsightX.Domain.ValueObjects;

namespace InsightX.Infrastructure.Anomaly.Rules
{
    public class TrendRule : IAnomalyRule
    {
        private readonly IMetricsRepository _metricsRepository;

        public TrendRule(IMetricsRepository metricsRepository)
        {
            _metricsRepository = metricsRepository;
        }

        public async Task<AnomalyResult> CheckAsync(int companyId, string kpiName, decimal currentValue, KPIConfig config)
        {
            var lastMonths = await _metricsRepository.GetLastNMonthsAsync(companyId, kpiName, config.TrendMonthsCount);
            if (lastMonths.Count < config.TrendMonthsCount) return AnomalyResult.None();

            lastMonths.Reverse();

            // Include current value to see if trend continues
            var allValues = new List<decimal>(lastMonths) { currentValue };

            var isDownwardTrend = true;
            var isUpwardTrend = true;

            for (int i = 0; i < allValues.Count - 1; i++)
            {
                if (allValues[i] <= allValues[i + 1])
                {
                    isDownwardTrend = false;
                }
                if (allValues[i] >= allValues[i + 1])
                {
                    isUpwardTrend = false;
                }
            }

            var isAnomaly = config.ThresholdDirection == ThresholdDirection.Below
                ? isDownwardTrend
                : isUpwardTrend;

            if (isAnomaly)
                return new AnomalyResult
                {
                    IsAnomaly = true,
                    Level = AnomalyLevel.Trend,
                    KPIName = kpiName,
                    CurrentValue = currentValue,
                    Threshold = config.Threshold
                };

            return AnomalyResult.None();
        }
    }
}
