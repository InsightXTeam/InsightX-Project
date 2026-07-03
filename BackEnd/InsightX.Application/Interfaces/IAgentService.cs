using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IAgentService
    {
        Task<ChatResponseDto> ChatAsync(ChatRequestDto request);
        Task<List<ChatHistoryDto>> GetHistoryAsync();
        Task ClearHistoryAsync();
    }
}
