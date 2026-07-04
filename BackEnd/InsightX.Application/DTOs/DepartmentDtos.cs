namespace InsightX.Application.DTOs
{
    public record CreateDepartmentDto(string Name);
    public record DepartmentResponseDto(int Id, string Name, int CompanyId, string? ManagerName = null, string? ManagerId = null);
    public record UpdateDepartmentDto(string Name, string? ManagerId = null);
}
