namespace InsightX.Application.DTOs
{
    public record InviteUserDto(string Name, string Email, string Password, int DepartmentId);
    public record UserResponseDto(string Id, string Name, string Email, string Role, int? DepartmentId, string? DepartmentName);
    public record OwnerManagementDto(
        string UserId,
        string OwnerName,
        string OwnerEmail,
        bool IsActivated,
        int CompanyId,
        string CompanyName,
        System.DateTime CompanyCreatedAt
    );

    public record ChangePasswordDto(string CurrentPassword, string NewPassword);
}
