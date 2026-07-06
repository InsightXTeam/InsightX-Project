using InsightX.Application.Interfaces;

namespace InsightX.Application.UseCases.Alerts
{
    public class MarkAlertSeenUseCase : IMarkAlertSeenUseCase
    {
        private readonly IAlertRepository _alertRepository;

        public MarkAlertSeenUseCase(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        public async Task ExecuteAsync(int alertId, CancellationToken cancellationToken = default)
        {
            await _alertRepository.MarkAsSeenAsync(alertId, cancellationToken);
        }
    }
}
