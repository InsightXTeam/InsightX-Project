using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IAgentService
    {
        Task<ChatResponseDto> SendMessageAsync(int companyId, string userId, ChatRequestDto request);
        Task<List<ChatResponseDto>> GetHistoryAsync(int companyId, Guid sessionId);
        Task<List<ChatSessionDto>> GetSessionsAsync(int companyId, string userId);
        Task DeleteSessionAsync(int companyId, string userId, Guid sessionId);
    }
}
