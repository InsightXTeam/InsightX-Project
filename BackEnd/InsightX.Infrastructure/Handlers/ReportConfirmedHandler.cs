using InsightX.Application.Interfaces;
using InsightX.Application.UseCases.Alerts;

namespace InsightX.Infrastructure.Handlers
{
    public class ReportConfirmedHandler : IReportConfirmedHandler
    {
        private readonly IGenerateAlertUseCase _generateAlert;

        public ReportConfirmedHandler(IGenerateAlertUseCase generateAlert)
        {
            _generateAlert = generateAlert;
        }

        public async Task HandleAsync(int companyId, int? departmentId, string kpiName, decimal value)
        {
            await _generateAlert.ExecuteAsync(companyId, departmentId, kpiName, value);
        }
    }
}
