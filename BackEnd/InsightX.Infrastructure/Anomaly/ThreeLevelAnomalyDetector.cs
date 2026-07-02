using InsightX.Application.Interfaces;
using InsightX.Domain.ValueObjects;

namespace InsightX.Infrastructure.Anomaly
{
    public class ThreeLevelAnomalyDetector : IAnomalyDetector
    {
        private readonly IEnumerable<IAnomalyRule> _rules;

        public ThreeLevelAnomalyDetector(IEnumerable<IAnomalyRule> rules)
        {
            _rules = rules;
        }

        public async Task<AnomalyResult> CheckAsync(int companyId, int departmentId, string kpiName, decimal currentValue)
        {
            foreach (var rule in _rules)
            {
                var result = await rule.CheckAsync(companyId, kpiName, currentValue);
                if (result.IsAnomaly) return result;
            }

            return AnomalyResult.None();
        }
    }
}
