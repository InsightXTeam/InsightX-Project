using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using InsightX.Domain.Enums;

namespace InsightX.Application.UseCases.Alerts
{
    public class CreateMonthlyReminderUseCase : ICreateMonthlyReminderUseCase
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IMetricsRepository _metricsRepository;

        public CreateMonthlyReminderUseCase(
            IAlertRepository alertRepository,
            IMetricsRepository metricsRepository)
        {
            _alertRepository = alertRepository;
            _metricsRepository = metricsRepository;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var companyIds = await _metricsRepository.GetAllCompanyIdsAsync();

            var nextMonth = DateTime.UtcNow.AddMonths(1);
            var monthName = nextMonth.ToString("MMMM yyyy");

            var reminders = new List<Alert>();

            foreach (var companyId in companyIds)
            {
                var reminder = new Alert
                {
                    CompanyId = companyId,
                    DepartmentId = 0,
                    KPIName = "Monthly Report Upload",
                    CurrentValue = 0,
                    Threshold = 0,
                    Message = $"Reminder: Please upload your {monthName} performance report before month end.",
                    Recommendation = "Log in and upload your monthly KPI report to keep the system up to date and enable anomaly detection.",
                    CreatedAt = DateTime.UtcNow,
                    SeenByOwner = false,
                    AlertType = AlertType.MonthlyReminder
                };

                reminders.Add(reminder);
            }

            if (reminders.Any())
            {
                await _alertRepository.AddRangeAsync(reminders, cancellationToken);
            }
        }
    }
}
