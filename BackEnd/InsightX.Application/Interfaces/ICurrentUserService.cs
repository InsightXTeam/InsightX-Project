namespace InsightX.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        int CompanyId { get; }
        string Role { get; } // "Owner" or "Manager"
        int? DepartmentId { get; } // null for Owner, set for Manager
    }
}
