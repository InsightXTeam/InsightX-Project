namespace InsightX.Application.Interfaces
{
    public interface IMarkAllAlertsSeenUseCase
    {
        Task ExecuteAsync(int companyId, int? departmentId = null, CancellationToken cancellationToken = default);
    }
}
