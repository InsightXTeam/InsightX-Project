using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Application.Services
{
    public class AgentService : IAgentService
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRagService _ragService;
        private readonly ILlmChatService _llmChatService;

        public AgentService(
            IAppDbContext dbContext,
            ICurrentUserService currentUserService,
            IRagService ragService,
            ILlmChatService llmChatService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _ragService = ragService;
            _llmChatService = llmChatService;
        }

        public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request)
        {
            var companyId = _currentUserService.CompanyId;
            var userId = _currentUserService.UserId;

            var chunks = await _ragService.RetrieveRelevantChunksAsync(request.Question, companyId, 5);
            var answer = await _llmChatService.GenerateAnswerAsync(request.Question, chunks);

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                UserId = userId,
                Question = request.Question,
                Answer = answer,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Conversations.Add(conversation);
            await _dbContext.SaveChangesAsync();

            return new ChatResponseDto
            {
                Answer = answer,
                Sources = chunks.ToList()
            };
        }

        public async Task<List<ChatHistoryDto>> GetHistoryAsync()
        {
            var userId = _currentUserService.UserId;
            var companyId = _currentUserService.CompanyId;

            return await _dbContext.Conversations
                .Where(c => c.UserId == userId && c.CompanyId == companyId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new ChatHistoryDto
                {
                    Id = c.Id,
                    Question = c.Question,
                    Answer = c.Answer,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task ClearHistoryAsync()
        {
            var userId = _currentUserService.UserId;
            var companyId = _currentUserService.CompanyId;

            var history = await _dbContext.Conversations
                .Where(c => c.UserId == userId && c.CompanyId == companyId)
                .ToListAsync();

            if (history.Count > 0)
            {
                _dbContext.Conversations.RemoveRange(history);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
