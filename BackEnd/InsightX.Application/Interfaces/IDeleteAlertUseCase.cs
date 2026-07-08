namespace InsightX.Application.Interfaces
{
    public interface IDeleteAlertUseCase
    {
        Task ExecuteAsync(int alertId, int companyId, int? departmentId, string role, CancellationToken cancellationToken = default);
    }
}
