using System;

namespace InsightX.Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public int UserId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
