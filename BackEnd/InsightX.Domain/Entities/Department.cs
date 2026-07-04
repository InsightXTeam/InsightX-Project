using System.Collections.Generic;

namespace InsightX.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public ApplicationUser? User { get; set; }
    }
}
