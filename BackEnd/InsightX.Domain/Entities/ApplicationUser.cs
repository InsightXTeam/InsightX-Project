using Microsoft.AspNetCore.Identity;

namespace InsightX.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public int CompanyId { get; set; }
        public int? DepartmentId { get; set; }

        public bool IsActivated { get; set; } = false;

        // Navigation
        public Company Company { get; set; } = null!;
        public Department? Department { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
