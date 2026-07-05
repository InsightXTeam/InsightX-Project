using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(dto, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(dto, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto dto, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshAsync(dto, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.LogoutAsync(userId, cancellationToken);
            return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result.Error);
        }
    }
}
