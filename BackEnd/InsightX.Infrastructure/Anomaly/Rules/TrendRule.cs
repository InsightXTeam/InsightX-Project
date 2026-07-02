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

        public async Task<AnomalyResult> CheckAsync(int companyId, string kpiName, decimal currentValue)
        {
            var config = await _metricsRepository
                .GetKPIConfigAsync(companyId, kpiName);

            if (config == null)
                return AnomalyResult.None();

            var lastMonths = await _metricsRepository.GetLastNMonthsAsync(companyId, kpiName, config.TrendMonthsCount);

            if (lastMonths.Count < config.TrendMonthsCount)
                return AnomalyResult.None();

            // GetLastNMonthsAsync returns data DESC (most-recent first),
            // reverse to get chronological order (oldest → newest) before trend check.
            lastMonths.Reverse();

            var isDownwardTrend = true;

            for (int i = 0; i < lastMonths.Count - 1; i++)
            {
                // Each month must be strictly greater than the next for a downward trend
                if (lastMonths[i] <= lastMonths[i + 1])
                {
                    isDownwardTrend = false;
                    break;
                }
            }

            if (isDownwardTrend)
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
