using System;

namespace InsightX.Domain.Entities
{
    public class Conversation
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public int CompanyId { get; set; }
        public string UserId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }

        public Company Company { get; set; }
        public ApplicationUser User { get; set; }
    }
}
