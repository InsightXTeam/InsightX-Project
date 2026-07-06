using InsightX.Application.UseCases.Alerts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InsightX.Infrastructure.BackgroundServices
{
    public class MonthlyReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyReminderBackgroundService> _logger;

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

        private static TimeSpan GetDelayUntilNextReminder()
        {
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, ReminderDay, 8, 0, 0, DateTimeKind.Utc);

            if (now >= nextRun)
                nextRun = nextRun.AddMonths(1);

            return nextRun - now;
        }

        private async Task SendRemindersAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sending monthly report reminders to all companies...");
            try
            {
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
