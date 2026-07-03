using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(IAppDbContext dbContext, IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CompanyId = user.CompanyId,
                DepartmentId = user.DepartmentId
            };

            return new LoginResponseDto
            {
                AccessToken = _jwtTokenService.GenerateAccessToken(profile),
                RefreshToken = _jwtTokenService.GenerateRefreshToken(),
                User = profile
            };
        }
    }
}
