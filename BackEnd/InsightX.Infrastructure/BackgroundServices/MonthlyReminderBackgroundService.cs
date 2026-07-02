using InsightX.Application.UseCases.Alerts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InsightX.Infrastructure.BackgroundServices
{
    /// <summary>
    /// A background service that fires on the 25th of every month to remind
    /// company owners to upload their monthly report before month-end.
    /// </summary>
    public class MonthlyReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyReminderBackgroundService> _logger;

        // Day of month to send the reminder (25th gives users ~5 days to upload)
        private const int ReminderDay = 25;

        public MonthlyReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonthlyReminderBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextReminder();
                _logger.LogInformation(
                    "Next monthly report reminder scheduled in {Days}d {Hours}h.",
                    (int)delay.TotalDays, delay.Hours);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                await SendRemindersAsync(stoppingToken);
            }
        }

        /// <summary>
        /// Calculates how long until the next 25th of the month at 8:00 AM UTC.
        /// If the 25th has already passed this month, schedules for next month's 25th.
        /// </summary>
        private static TimeSpan GetDelayUntilNextReminder()
        {
            var now = DateTime.UtcNow;

            // Target: 25th of current month at 8 AM UTC
            var nextRun = new DateTime(now.Year, now.Month, ReminderDay, 8, 0, 0, DateTimeKind.Utc);

            // If today is past the 25th, move to next month
            if (now >= nextRun)
                nextRun = nextRun.AddMonths(1);

            return nextRun - now;
        }

        private async Task SendRemindersAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sending monthly report reminders to all companies...");
            try
            {
                // BackgroundService must create its own scope for scoped services (DbContext etc.)
                using var scope = _scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<CreateMonthlyReminderUseCase>();
                await useCase.ExecuteAsync();
                _logger.LogInformation("Monthly report reminders sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending monthly report reminders.");
            }
        }
    }
}
