namespace InsightX.Application.Interfaces
{
    public interface IMarkAlertSeenUseCase
    {
        Task ExecuteAsync(int id, CancellationToken cancellationToken = default);
    }
}
