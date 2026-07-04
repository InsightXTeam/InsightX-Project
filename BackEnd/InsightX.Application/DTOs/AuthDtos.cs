namespace InsightX.Application.DTOs
{
    public record RegisterDto(string CompanyName, string OwnerName, string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record RefreshDto(string AccessToken, string RefreshToken);
    public record AuthResponseDto(string AccessToken, string RefreshToken);
}
