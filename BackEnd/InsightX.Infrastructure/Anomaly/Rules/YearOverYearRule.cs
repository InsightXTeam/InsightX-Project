using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Enums;
using InsightX.Domain.ValueObjects;

namespace InsightX.Infrastructure.Anomaly.Rules
{
    public class YearOverYearRule : IAnomalyRule
    {
        private readonly IMetricsRepository _metricsRepository;

        public YearOverYearRule(IMetricsRepository metricsRepository)
        {
            _metricsRepository = metricsRepository;
        }

        public async Task<AnomalyResult> CheckAsync(int companyId, string kpiName, decimal currentValue, KPIConfig config)
        {
            var lastYearValue = await _metricsRepository
                .GetSameMonthLastYearAsync(companyId, kpiName, DateTime.UtcNow.Month);

            if (lastYearValue is not { } lyv || lyv <= 0)
                return AnomalyResult.None();

            var percentDiff = Math.Abs(currentValue - lyv) / lyv * 100;

            if (percentDiff > config.AlertPercentageDiff)
                return new AnomalyResult
                {
                    IsAnomaly = true,
                    Level = AnomalyLevel.YearOverYear,
                    KPIName = kpiName,
                    CurrentValue = currentValue,
                    Threshold = lyv
                };
            return AnomalyResult.None();
        }
    }
}
