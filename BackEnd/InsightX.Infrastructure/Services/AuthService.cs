using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs.Auth;
using InsightX.Application.Interfaces.Auth;
using InsightX.Domain.Entities;
using InsightX.Domain.Entities.Auth;
using InsightX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
                // Verify if email already registered
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<object>.Fail(400, "Email already registered.");
                }

                // 1. Create Company
                var company = new Company
                {
                    Name = dto.CompanyName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // 2. Create ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    Name = dto.OwnerName,
                    CompanyId = company.Id
                };

                // 3. Hash password via UserManager
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ServiceResult<object>.Fail(400, errors);
                }

                // Ensure "Owner" role exists and add user to it
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
                Token = refreshTokenString,
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

            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken
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
                Token = newRefreshTokenString,
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
