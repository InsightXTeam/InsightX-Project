using System;
using System.Collections.Generic;
using InsightX.Application.Interfaces;

namespace InsightX.Application.DTOs
{
    public class ChatRequestDto
    {
        public string Question { get; set; } = string.Empty;
    }

    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public List<RagChunk> Sources { get; set; } = new List<RagChunk>();
    }

    public class ChatHistoryDto
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
