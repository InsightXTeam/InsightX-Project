namespace InsightX.Application.Interfaces
{
    public interface ICreateMonthlyReminderUseCase
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
