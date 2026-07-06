using System.Security.Claims;

namespace InsightX.Application.Extensions
{
    public static class ClaimsExtensions
    {
        public static int GetCompanyId(this ClaimsPrincipal user)
        {
            var companyIdClaim = user.FindFirst("CompanyId")?.Value;

            if (!int.TryParse(companyIdClaim, out var companyId))
                throw new UnauthorizedAccessException("CompanyId is missing or invalid.");

            return companyId;
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId ?? throw new UnauthorizedAccessException("UserId is missing or invalid.");
        }

        public static int? GetDepartmentId(this ClaimsPrincipal user)
        {
            var departmentIdClaim = user.FindFirst("DepartmentId")?.Value;

            if (string.IsNullOrEmpty(departmentIdClaim))
                return null;

            if (int.TryParse(departmentIdClaim, out var departmentId))
                return departmentId;

            return null;
        }
    }
}