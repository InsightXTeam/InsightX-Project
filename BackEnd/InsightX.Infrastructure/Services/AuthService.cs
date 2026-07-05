using InsightX.Application.Common;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using InsightX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace InsightX.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IOptions<JwtOptions> _jwtOptions;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            ITokenService tokenService,
            IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
            _jwtOptions = jwtOptions;
        }

        public async Task<ServiceResult<object>> RegisterAsync(RegisterDto dto)
        {
            if (dto == null)
            {
                return ServiceResult<object>.Fail(400, "Invalid request.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<object>.Fail(400, "Email already registered.");
                }


                var company = new Company
                {
                    Name = dto.CompanyName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    Name = dto.OwnerName,
                    CompanyId = company.Id
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ServiceResult<object>.Fail(400, errors);
                }

                if (!await _roleManager.RoleExistsAsync("Owner"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Owner"));
                }
                await _userManager.AddToRoleAsync(user, "Owner");

                await transaction.CommitAsync();

                return ServiceResult<object>.Success(new { Message = "Registration successful. Please wait for the Super Admin to activate your account." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<object>.Fail(500, $"An error occurred during registration: {ex.Message}");
            }
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            if (dto == null)
            {
                return ServiceResult<AuthResponseDto>.Fail(400, "Invalid request.");
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return ServiceResult<AuthResponseDto>.Fail(401, "Invalid credentials.");
            }

            if (!user.IsActivated)
            {
                return ServiceResult<AuthResponseDto>.Fail(400, "Your account is not activated yet. Please wait for Super Admin activation.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            var expiryDays = _jwtOptions.Value.RefreshTokenExpiryDays;
            var refreshTokenEntity = new RefreshToken
            {
                Token = TokenHasher.Hash(refreshTokenString),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto(accessToken, refreshTokenString));
        }

        public async Task<ServiceResult<AuthResponseDto>> RefreshAsync(RefreshDto dto)
        {
            if (dto == null)
            {
                return ServiceResult<AuthResponseDto>.Fail(400, "Invalid request.");
            }

            ClaimsPrincipal principal;
            try
            {
                principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            }
            catch (Exception)
            {
                return ServiceResult<AuthResponseDto>.Fail(400, "Invalid access token.");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return ServiceResult<AuthResponseDto>.Fail(400, "Invalid claims.");
            }

            var hashedToken = TokenHasher.Hash(dto.RefreshToken);
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == hashedToken
                    && r.UserId == userId
                    && !r.IsRevoked
                    && r.ExpiresAt > DateTime.UtcNow);

            if (stored == null)
            {
                return ServiceResult<AuthResponseDto>.Fail(401, "Invalid or expired refresh token.");
            }

            stored.IsRevoked = true;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<AuthResponseDto>.Fail(404, "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();

            var expiryDays = _jwtOptions.Value.RefreshTokenExpiryDays;
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = TokenHasher.Hash(newRefreshTokenString),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto(newAccessToken, newRefreshTokenString));
        }

        public async Task<ServiceResult> LogoutAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return ServiceResult.Fail(401, "Unauthorized");
            }

            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();

            tokens.ForEach(t => t.IsRevoked = true);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }
}
