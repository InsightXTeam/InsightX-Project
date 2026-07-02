namespace InsightX.Application.Interfaces
{
    public interface IReportConfirmedHandler
    {
        Task HandleAsync(int companyId, int departmentId, string kpiName, decimal value);
    }
}
