using System;
using System.Collections.Generic;

namespace InsightX.Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ApplicationUser> Users { get; set; } = [];
        public ICollection<Department> Departments { get; set; } = [];
        public ICollection<KPI> KPIs { get; set; } = [];
    }
}
