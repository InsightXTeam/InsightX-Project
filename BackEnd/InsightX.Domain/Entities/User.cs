namespace InsightX.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Owner" or "Manager"
        
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
