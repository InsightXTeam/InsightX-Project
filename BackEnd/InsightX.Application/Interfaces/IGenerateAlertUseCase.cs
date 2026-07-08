namespace InsightX.Application.Interfaces
{
    public interface IGenerateAlertUseCase
    {
        Task ExecuteAsync(int companyId, int? departmentId, string kpiName, decimal currentValue, int? reportId = null, CancellationToken cancellationToken = default);
    }
}
