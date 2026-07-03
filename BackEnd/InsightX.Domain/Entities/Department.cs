using System.Collections.Generic;

namespace InsightX.Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
