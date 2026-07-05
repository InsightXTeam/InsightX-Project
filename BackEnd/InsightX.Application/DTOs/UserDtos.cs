using System.ComponentModel.DataAnnotations;

namespace InsightX.Application.DTOs
{
    public record InviteUserDto(
        [Required, MaxLength(100)] string Name,
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required, Range(1, int.MaxValue, ErrorMessage = "A valid department is required.")] int DepartmentId
    );
    public record UserResponseDto(string Id, string Name, string Email, string Role, int? DepartmentId, string? DepartmentName);
    public record OwnerManagementDto(
        string UserId,
        string OwnerName,
        string OwnerEmail,
        bool IsActivated,
        int CompanyId,
        string CompanyName,
        DateTime CompanyCreatedAt
    );

    public record ChangePasswordDto(
        [Required] string CurrentPassword,
        [Required, MinLength(8)] string NewPassword
    );
    public record UpdateUserDepartmentDto(int? DepartmentId);
}
