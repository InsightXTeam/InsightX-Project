using InsightX.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace InsightX.Application.UseCases.Alerts
{
    public class DeleteAlertUseCase : IDeleteAlertUseCase
    {
        private readonly IAlertRepository _alertRepository;

        public DeleteAlertUseCase(IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

        public async Task ExecuteAsync(int alertId, int companyId, int? departmentId, string role, CancellationToken cancellationToken = default)
        {
            var alert = await _alertRepository.GetByIdAsync(alertId);
            if (alert == null) return;

            // Security check: ensure the alert belongs to the company
            if (alert.CompanyId != companyId) return;

            // If manager, ensure they can only delete alerts for their department
            if (role == "Manager" && departmentId.HasValue && alert.DepartmentId.HasValue && alert.DepartmentId.Value != departmentId.Value)
            {
                return;
            }

            await _alertRepository.DeleteAsync(alert, cancellationToken);
        }
    }
}
