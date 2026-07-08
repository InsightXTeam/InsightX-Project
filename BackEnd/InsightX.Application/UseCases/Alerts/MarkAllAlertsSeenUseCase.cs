using InsightX.Application.Interfaces;

namespace InsightX.Application.UseCases.Alerts
{
    public class MarkAllAlertsSeenUseCase : IMarkAllAlertsSeenUseCase
    {
        private readonly IAlertRepository _alertRepository;

        public MarkAllAlertsSeenUseCase(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        public async Task ExecuteAsync(int companyId, int? departmentId = null, CancellationToken cancellationToken = default)
        {
            await _alertRepository.MarkAllAsSeenAsync(companyId, departmentId, cancellationToken);
        }
    }
}
