using System.ComponentModel.DataAnnotations;

namespace InsightX.Application.DTOs
{
    public record CreateDepartmentDto([Required, MaxLength(100)] string Name);
    public record DepartmentResponseDto(int Id, string Name, int CompanyId, string? ManagerName = null, string? ManagerId = null);
    public record UpdateDepartmentDto([Required, MaxLength(100)] string Name, string? ManagerId = null);
}
