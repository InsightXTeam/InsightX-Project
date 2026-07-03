using System.Security.Claims;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InsightX.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int UserId => GetIntClaim(ClaimTypes.NameIdentifier, 1);

        public int CompanyId => GetIntClaim("CompanyId", 1);

        public string Role => GetClaim(ClaimTypes.Role, "Owner");

        public int? DepartmentId
        {
            get
            {
                var value = User?.FindFirst("DepartmentId")?.Value;
                return int.TryParse(value, out var deptId) ? deptId : null;
            }
        }

        private int GetIntClaim(string claimType, int fallback)
        {
            var value = User?.FindFirst(claimType)?.Value;
            return int.TryParse(value, out var id) ? id : fallback;
        }

        private string GetClaim(string claimType, string fallback)
        {
            return User?.FindFirst(claimType)?.Value ?? fallback;
        }
    }
}
