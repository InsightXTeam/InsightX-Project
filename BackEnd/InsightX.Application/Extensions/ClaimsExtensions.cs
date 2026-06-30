using System.Security.Claims;

namespace InsightX.Application.Extensions
{
    public static class ClaimsExtensions
    {
        public static int GetCompanyId(this ClaimsPrincipal user)
        {
            var companyIdClaim = user.FindFirst("CompanyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : 0;
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
    }
}
