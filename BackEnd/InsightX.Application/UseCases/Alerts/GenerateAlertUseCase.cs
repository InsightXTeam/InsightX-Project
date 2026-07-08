using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;

namespace InsightX.Application.UseCases.Alerts
{
    public class GenerateAlertUseCase : IGenerateAlertUseCase
    {
        private readonly IAnomalyDetector _detector;
        private readonly IAlertMessageGenerator _messageGenerator;
        private readonly IAlertRepository _alertRepository;
        private readonly IMetricsRepository _metricsRepository;

        public GenerateAlertUseCase(IAnomalyDetector detector,
            IAlertMessageGenerator messageGenerator,
            IAlertRepository alertRepository,
            IMetricsRepository metricsRepository)
        {
            _detector = detector;
            _messageGenerator = messageGenerator;
            _alertRepository = alertRepository;
            _metricsRepository = metricsRepository;
        }

        public async Task ExecuteAsync(
            int companyId,
            int? departmentId,
            string kpiName,
            decimal currentValue,
            int? reportId = null,
            CancellationToken cancellationToken = default)
        {
            // 1- detecting if there any problems
            var result = await _detector.CheckAsync(companyId, departmentId, kpiName, currentValue);
            if (!result.IsAnomaly) return;

            // 2- get history context for ai
            var history = await _metricsRepository.GetLastNMonthsAsync(companyId, kpiName, 3);
            var historyContext = string.Join(", ", history);

            // 3- make ai generate message
            var (message, recommendation) = await _messageGenerator.GenerateAsync(kpiName, result.CurrentValue, result.Threshold, historyContext);

            // 4- store alert in DB
            var alert = new Alert()
            {
                CompanyId = companyId,
                DepartmentId = departmentId,
                ReportId = reportId,
                KPIName = kpiName,
                CurrentValue = result.CurrentValue,
                Threshold = result.Threshold,
                Message = message,
                Recommendation = recommendation,
                CreatedAt = DateTime.UtcNow,
                SeenByOwner = false
            };

            await _alertRepository.AddAsync(alert);
        }
    }
}
