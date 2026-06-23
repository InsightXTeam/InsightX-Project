namespace InsightX.Application.DTOs
{
    public record CreateDepartmentDto(string Name);
    public record DepartmentResponseDto(int Id, string Name, int CompanyId);
}
