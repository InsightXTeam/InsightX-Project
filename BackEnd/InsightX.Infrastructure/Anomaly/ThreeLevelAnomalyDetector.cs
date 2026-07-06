using InsightX.Application.Interfaces;
using InsightX.Domain.ValueObjects;

namespace InsightX.Infrastructure.Anomaly
{
    public class ThreeLevelAnomalyDetector : IAnomalyDetector
    {
        private readonly IEnumerable<IAnomalyRule> _rules;
        private readonly IMetricsRepository _metricsRepository;

        public ThreeLevelAnomalyDetector(IEnumerable<IAnomalyRule> rules, IMetricsRepository metricsRepository)
        {
            _rules = rules;
            _metricsRepository = metricsRepository;
        }

        public async Task<AnomalyResult> CheckAsync(int companyId, int? departmentId, string kpiName, decimal currentValue)
        {
            var config = await _metricsRepository.GetKPIConfigAsync(companyId, kpiName);
            if (config == null) return AnomalyResult.None();

            foreach (var rule in _rules)
            {
                var result = await rule.CheckAsync(companyId, kpiName, currentValue, config);
                if (result.IsAnomaly) return result;
            }

            return AnomalyResult.None();
        }
    }
}
