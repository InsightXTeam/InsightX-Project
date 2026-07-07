using System;
using System.ComponentModel.DataAnnotations;

namespace InsightX.Application.DTOs
{
    public class ChatRequestDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public Guid SessionId { get; set; }
    }

    public class ChatResponseDto
    {
        public Guid SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Sender { get; set; } = "AI";
        public DateTime CreatedAt { get; set; }
    }

    public class ChatSessionDto
    {
        public Guid SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
