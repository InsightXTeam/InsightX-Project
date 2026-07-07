using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities;
using InsightX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace InsightX.Infrastructure.Services
{
    public class AgentService : IAgentService
    {
        private readonly IDashboardService _dashboardService;
        private readonly AppDbContext _context;
        private readonly IChatCompletionService _chatService;

        public AgentService(
            IDashboardService dashboardService, 
            AppDbContext context,
            IChatCompletionService chatService)
        {
            _dashboardService = dashboardService;
            _context = context;
            _chatService = chatService;
        }

        public async Task<List<ChatResponseDto>> GetHistoryAsync(int companyId, Guid sessionId)
        {
            var history = await _context.Conversations
                .Where(c => c.SessionId == sessionId && c.CompanyId == companyId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<ChatResponseDto>();
            foreach(var conv in history)
            {
                result.Add(new ChatResponseDto { SessionId = conv.SessionId, Message = conv.Question, Sender = "User", CreatedAt = conv.CreatedAt });
                result.Add(new ChatResponseDto { SessionId = conv.SessionId, Message = conv.Answer, Sender = "AI", CreatedAt = conv.CreatedAt.AddMilliseconds(1) });
            }

            return result;
        }

        public async Task<List<ChatSessionDto>> GetSessionsAsync(int companyId, string userId)
        {
            // Group by SessionId and find the very first Conversation Id (integer) for each session
            var firstMessageIds = await _context.Conversations
                .Where(c => c.CompanyId == companyId && c.UserId == userId)
                .GroupBy(c => c.SessionId)
                .Select(g => g.Min(c => c.Id))
                .ToListAsync();

            // Fetch the actual session details using the precise integer Ids
            var sessions = await _context.Conversations
                .Where(c => firstMessageIds.Contains(c.Id))
                .Select(c => new ChatSessionDto
                {
                    SessionId = c.SessionId,
                    Title = c.Question ?? "New Chat",
                    CreatedAt = c.CreatedAt
                })
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return sessions;
        }

        public async Task DeleteSessionAsync(int companyId, string userId, Guid sessionId)
        {
            var conversations = await _context.Conversations
                .Where(c => c.SessionId == sessionId && c.CompanyId == companyId && c.UserId == userId)
                .ToListAsync();

            if (conversations.Any())
            {
                _context.Conversations.RemoveRange(conversations);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ChatResponseDto> SendMessageAsync(int companyId, string userId, ChatRequestDto request)
        {
            // 1. Fetch live context from DashboardService
            var kpis = await _dashboardService.GetKpisSummaryAsync(companyId, null);
            var alerts = await _dashboardService.GetRecentAlertsAsync(companyId, null);
            var depts = await _dashboardService.GetDepartmentsPerformanceAsync(companyId);

            var contextData = new
            {
                CompanyKPIs = kpis,
                RecentAlerts = alerts,
                Departments = depts
            };
            var contextJson = JsonSerializer.Serialize(contextData, new JsonSerializerOptions { WriteIndented = true });

            // 2. Build Chat History
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage($@"
You are InsightX AI, a friendly and expert business analyst assistant.
Your goal is to help the user understand their business performance by answering their questions in a natural, conversational, and user-friendly way.
Do NOT mention technical terms like 'JSON', 'arrays', 'data context', or 'dashboard data objects'. Simply speak about the metrics, departments, and alerts as if you are a human analyst presenting a report to a business user.
If the answer is not provided in the context below, state that clearly and politely without making up information.
Use Markdown formatting (like bolding and lists) to make your response easy to read.

BUSINESS CONTEXT:
{contextJson}
");

            // 3. Load previous conversation
            var pastConversations = await _context.Conversations
                .Where(c => c.SessionId == request.SessionId && c.CompanyId == companyId)
                .OrderBy(c => c.CreatedAt)
                .Take(10)
                .ToListAsync();

            foreach(var conv in pastConversations)
            {
                chatHistory.AddUserMessage(conv.Question);
                chatHistory.AddAssistantMessage(conv.Answer);
            }

            // 4. Add new user message
            chatHistory.AddUserMessage(request.Message);

            // 5. Invoke Semantic Kernel Chat Service
            var responseContents = await _chatService.GetChatMessageContentsAsync(chatHistory);
            var aiResponse = responseContents.FirstOrDefault()?.Content ?? "I'm sorry, I couldn't generate a response.";

            // 6. Save to DB
            var conversation = new Conversation
            {
                SessionId = request.SessionId,
                CompanyId = companyId,
                UserId = userId,
                Question = request.Message,
                Answer = aiResponse,
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return new ChatResponseDto
            {
                SessionId = request.SessionId,
                Message = aiResponse,
                Sender = "AI",
                CreatedAt = conversation.CreatedAt
            };
        }

        public async IAsyncEnumerable<string> SendMessageStreamAsync(int companyId, string userId, ChatRequestDto request)
        {
            var kpis = await _dashboardService.GetKpisSummaryAsync(companyId, null);
            var alerts = await _dashboardService.GetRecentAlertsAsync(companyId, null);
            var depts = await _dashboardService.GetDepartmentsPerformanceAsync(companyId);

            var contextData = new
            {
                CompanyKPIs = kpis,
                RecentAlerts = alerts,
                Departments = depts
            };
            var contextJson = JsonSerializer.Serialize(contextData, new JsonSerializerOptions { WriteIndented = true });

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage($@"
You are InsightX AI, a friendly and expert business analyst assistant.
Your goal is to help the user understand their business performance by answering their questions in a natural, conversational, and user-friendly way.
Do NOT mention technical terms like 'JSON', 'arrays', 'data context', or 'dashboard data objects'. Simply speak about the metrics, departments, and alerts as if you are a human analyst presenting a report to a business user.
If the answer is not provided in the context below, state that clearly and politely without making up information.
Use Markdown formatting (like bolding and lists) to make your response easy to read.

BUSINESS CONTEXT:
{contextJson}
");

            var pastConversations = await _context.Conversations
                .Where(c => c.SessionId == request.SessionId && c.CompanyId == companyId)
                .OrderBy(c => c.CreatedAt)
                .Take(10)
                .ToListAsync();

            foreach(var conv in pastConversations)
            {
                chatHistory.AddUserMessage(conv.Question);
                chatHistory.AddAssistantMessage(conv.Answer);
            }

            chatHistory.AddUserMessage(request.Message);

            var fullResponse = new System.Text.StringBuilder();

            await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    fullResponse.Append(chunk.Content);
                    yield return chunk.Content;
                }
            }

            var aiResponse = fullResponse.ToString();
            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                aiResponse = "I'm sorry, I couldn't generate a response.";
                yield return aiResponse;
            }

            var conversation = new Conversation
            {
                SessionId = request.SessionId,
                CompanyId = companyId,
                UserId = userId,
                Question = request.Message,
                Answer = aiResponse,
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
        }
    }
}
