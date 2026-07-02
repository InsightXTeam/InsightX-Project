using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;

namespace InsightX.Application.UseCases.Alerts
{
    public class GetAlertsUseCase
    {
        private readonly IAlertRepository _alertRepository;

        public GetAlertsUseCase(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        public async Task<List<AlertDto>> ExecuteAsync(int companyId, bool? seenFilter)
        {
            var alerts = await _alertRepository.GetByCompanyAsync(companyId, seenFilter);

            return alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                KPIName = a.KPIName,
                CurrentValue = a.CurrentValue,
                Threshold = a.Threshold,
                Message = a.Message,
                Recommendation = a.Recommendation,
                CreatedAt = a.CreatedAt,
                SeenByOwner = a.SeenByOwner,
                AlertType = a.AlertType
            }).ToList();
        }
    }
}
