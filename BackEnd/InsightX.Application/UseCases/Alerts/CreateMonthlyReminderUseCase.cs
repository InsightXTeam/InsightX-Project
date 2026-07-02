using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using InsightX.Domain.Enums;

namespace InsightX.Application.UseCases.Alerts
{
    /// <summary>
    /// Creates a monthly reminder alert for every company, prompting them to upload their report.
    /// Called by MonthlyReminderBackgroundService on the 25th of each month.
    /// </summary>
    public class CreateMonthlyReminderUseCase
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

        public async Task ExecuteAsync()
        {
            var companyIds = await _metricsRepository.GetAllCompanyIdsAsync();

            // The reminder is for NEXT month's report (upload before month ends)
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            var monthName = nextMonth.ToString("MMMM yyyy");

            foreach (var companyId in companyIds)
            {
                var reminder = new Alert
                {
                    CompanyId = companyId,
                    DepartmentId = 0,                 // 0 = system-level, not department-specific
                    KPIName = "Monthly Report Upload",
                    CurrentValue = 0,
                    Threshold = 0,
                    Message = $"Reminder: Please upload your {monthName} performance report before month end.",
                    Recommendation = "Log in and upload your monthly KPI report to keep the system up to date and enable anomaly detection.",
                    CreatedAt = DateTime.UtcNow,
                    SeenByOwner = false,
                    AlertType = AlertType.MonthlyReminder
                };

                await _alertRepository.AddAsync(reminder);
            }
        }
    }
}
