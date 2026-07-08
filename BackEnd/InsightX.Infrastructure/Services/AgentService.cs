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
using InsightXAI.Application.Interfaces.Rag;
using InsightXAI.Application.DTOs;

namespace InsightX.Infrastructure.Services
{
    public class AgentService : IAgentService
    {
        private readonly IDashboardService _dashboardService;
        private readonly AppDbContext _context;
        private readonly IChatCompletionService _chatService;
        private readonly IRetrieveChunksUseCase _retrieveChunksUseCase;

        public AgentService(
            IDashboardService dashboardService, 
            AppDbContext context,
            IChatCompletionService chatService,
            IRetrieveChunksUseCase retrieveChunksUseCase)
        {
            _dashboardService = dashboardService;
            _context = context;
            _chatService = chatService;
            _retrieveChunksUseCase = retrieveChunksUseCase;
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

        public async Task<ChatResponseDto> SendMessageAsync(int companyId, string userId, int? departmentId, ChatRequestDto request)
        {
            var systemPrompt = @"
You are InsightX AI, a friendly and expert business analyst assistant.
Your goal is to help the user understand their business performance.

You have access to the following tools to fetch live data:
- Action: GetKpisSummary
  Description: Fetches overall Key Performance Indicators (KPIs) for the company.
- Action: GetRecentAlerts
  Description: Fetches recent anomaly alerts and warnings for the company.
- Action: GetDepartmentsPerformance
  Description: Fetches performance breakdown by department.
- Action: GetTrends
  Description: Fetches historical trend data for KPIs over time.
- Action: GetDocumentInformation
  Description: Searches the vector database for information from company documents.
  Input: The search query string.

To use a tool, you MUST use the following exact format:
Thought: I need to check the recent alerts to answer the user's question.
Action: GetRecentAlerts
Action Input: (None)

Or for a tool that requires input:
Thought: I need to search the vector database for company policies on vacation.
Action: GetDocumentInformation
Action Input: company policies on vacation

Once you output 'Action: [ToolName]', STOP and wait for an Observation.

CRITICAL INSTRUCTIONS FOR YOUR FINAL ANSWER:
1. When you have enough information, you MUST output: 'Final Answer: [Your response]'
2. Your response must be extremely USER FRIENDLY. Speak like a human business analyst.
3. NEVER mention developer terms like 'JSON', 'arrays', 'tools', 'Action', 'Observation', or 'database'.
4. NEVER show the user the raw data format. Extract the insights and present them naturally.
5. Format numbers clearly (e.g., currency, percentages) and use Markdown (bolding, lists, tables) to make it easy to read.
";

            // 1. Build Chat History
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);

            // 2. Load previous conversation
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

            // 3. Add new user message
            chatHistory.AddUserMessage(request.Message);

            // 4. ReAct Loop
            string finalAnswer = string.Empty;
            int maxIterations = 5;
            
            for (int i = 0; i < maxIterations; i++)
            {
                var responseContents = await _chatService.GetChatMessageContentsAsync(chatHistory);
                var aiResponse = responseContents.FirstOrDefault()?.Content ?? "";
                
                chatHistory.AddAssistantMessage(aiResponse);

                if (aiResponse.Contains("Final Answer:"))
                {
                    finalAnswer = aiResponse.Substring(aiResponse.IndexOf("Final Answer:") + "Final Answer:".Length).Trim();
                    break;
                }
                
                if (aiResponse.Contains("Action:"))
                {
                    // Parse the action and optional input
                    var actionLines = aiResponse.Split('\n').Where(l => l.Trim().StartsWith("Action:")).ToList();
                    var actionInputLines = aiResponse.Split('\n').Where(l => l.Trim().StartsWith("Action Input:")).ToList();

                    if (actionLines.Any())
                    {
                        var action = actionLines.First().Replace("Action:", "").Trim();
                        var actionInput = actionInputLines.FirstOrDefault()?.Replace("Action Input:", "").Trim() ?? string.Empty;
                        string observation = "Action not recognized.";
                        
                        try
                        {
                            if (action == "GetKpisSummary")
                            {
                                var data = await _dashboardService.GetKpisSummaryAsync(companyId, departmentId);
                                observation = JsonSerializer.Serialize(data);
                            }
                            else if (action == "GetRecentAlerts")
                            {
                                var data = await _dashboardService.GetRecentAlertsAsync(companyId, departmentId);
                                observation = JsonSerializer.Serialize(data);
                            }
                            else if (action == "GetDepartmentsPerformance")
                            {
                                var data = await _dashboardService.GetDepartmentsPerformanceAsync(companyId);
                                if (departmentId.HasValue)
                                {
                                    data = data.Where(d => d.DepartmentId == departmentId.Value).ToList();
                                }
                                observation = JsonSerializer.Serialize(data);
                            }
                            else if (action == "GetTrends")
                            {
                                var data = await _dashboardService.GetTrendsAsync(companyId, departmentId);
                                observation = JsonSerializer.Serialize(data);
                            }
                            else if (action == "GetDocumentInformation")
                            {
                                if (string.IsNullOrWhiteSpace(actionInput) || actionInput == "(None)")
                                {
                                    observation = "Error: GetDocumentInformation requires a search query as Action Input.";
                                }
                                else
                                {
                                    var requestDto = new RetrieveRequestDto { Question = actionInput, TopK = 3 };
                                    var result = await _retrieveChunksUseCase.ExecuteAsync(requestDto, companyId, departmentId);
                                    if (result.IsSuccess && result.Data != null)
                                    {
                                        observation = JsonSerializer.Serialize(result.Data.Chunks);
                                    }
                                    else
                                    {
                                        observation = "No relevant documents found or error occurred.";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            observation = $"Error executing action: {ex.Message}";
                        }
                        
                        chatHistory.AddUserMessage($"Observation: {observation}");
                    }
                    else
                    {
                        chatHistory.AddUserMessage("Observation: No valid Action found. Please provide a Final Answer or a valid Action.");
                    }
                }
                else
                {
                    // No action and no final answer. Force it.
                    chatHistory.AddUserMessage("Observation: You didn't provide a 'Final Answer:' or an 'Action:'. Please do so.");
                }
            }

            if (string.IsNullOrWhiteSpace(finalAnswer))
            {
                finalAnswer = "I'm sorry, I couldn't generate a complete response in time.";
            }

            // 5. Save to DB
            var conversation = new Conversation
            {
                SessionId = request.SessionId,
                CompanyId = companyId,
                UserId = userId,
                Question = request.Message,
                Answer = finalAnswer,
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return new ChatResponseDto
            {
                SessionId = request.SessionId,
                Message = finalAnswer,
                Sender = "AI",
                CreatedAt = conversation.CreatedAt
            };
        }

        public async IAsyncEnumerable<string> SendMessageStreamAsync(int companyId, string userId, int? departmentId, ChatRequestDto request)
        {
            // Note: True streaming for a ReAct loop is complex because we buffer tool thoughts vs final answers.
            // Since ItiChatCompletionService falls back to non-streaming anyway, we execute the ReAct loop
            // and yield the final answer.
            var response = await SendMessageAsync(companyId, userId, departmentId, request);
            yield return response.Message;
        }
    }
}
