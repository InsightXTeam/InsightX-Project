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
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public static int GetDepartmentId(this ClaimsPrincipal user)
        {
            var departmentIdClaim = user.FindFirst("DepartmentId")?.Value;

            if (!int.TryParse(departmentIdClaim, out var departmentId))
                throw new UnauthorizedAccessException("DepartmentId is missing or invalid.");

            return departmentId;
        }
    }
}
