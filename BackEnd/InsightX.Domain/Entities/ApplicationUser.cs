using Microsoft.AspNetCore.Identity;

namespace InsightX.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public int CompanyId { get; set; }
        public int? DepartmentId { get; set; }

        public bool IsActivated { get; set; } = false;
        public bool MustChangePassword { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public Company Company { get; set; } = null!;
        public Department? Department { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
