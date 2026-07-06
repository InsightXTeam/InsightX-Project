namespace InsightX.Application.Interfaces
{
    public interface IAlertMessageGenerator
    {
        Task<(string message, string recommendation)> GenerateAsync(
            string kpiName,
            decimal currentValue,
            decimal threshold,
            string historyContext);
    }
}
