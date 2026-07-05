using System.ComponentModel.DataAnnotations;

namespace InsightX.Application.DTOs
{
    public record RegisterDto(
        [Required, MaxLength(200)] string CompanyName,
        [Required, MaxLength(100)] string OwnerName,
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password);
    public record LoginDto([Required, EmailAddress] string Email, [Required] string Password);
    public record RefreshDto([Required] string AccessToken, [Required] string RefreshToken);
    public record AuthResponseDto(string AccessToken, string RefreshToken, bool MustChangePassword);
}
