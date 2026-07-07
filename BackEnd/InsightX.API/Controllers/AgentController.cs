using System;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace InsightX.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/agent")]
    [ApiVersion("1.0")]
    [Authorize]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        private int GetCompanyId()
        {
            return int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        [HttpPost("chat")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            
            if (companyId == 0 || string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _agentService.SendMessageAsync(companyId, userId, request);
            return Ok(response);
        }

        [HttpPost("stream")]
        public async Task StreamMessage([FromBody] ChatRequestDto request)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            
            if (companyId == 0 || string.IsNullOrEmpty(userId))
            {
                Response.StatusCode = 401;
                return;
            }

            Response.ContentType = "text/event-stream";

            await foreach (var chunk in _agentService.SendMessageStreamAsync(companyId, userId, request))
            {
                // Format the string for SSE
                var data = $"data: {chunk.Replace("\n", "\\n")}\n\n";
                await Response.WriteAsync(data);
                await Response.Body.FlushAsync();
            }
        }

        [HttpGet("chat/{sessionId:guid}")]
        public async Task<IActionResult> GetHistory([FromRoute] Guid sessionId)
        {
            var companyId = GetCompanyId();
            
            if (companyId == 0)
                return Unauthorized();

            var response = await _agentService.GetHistoryAsync(companyId, sessionId);
            return Ok(response);
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            
            if (companyId == 0 || string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _agentService.GetSessionsAsync(companyId, userId);
            return Ok(response);
        }

        [HttpDelete("chat/{sessionId:guid}")]
        public async Task<IActionResult> DeleteSession([FromRoute] Guid sessionId)
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();
            
            if (companyId == 0 || string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _agentService.DeleteSessionAsync(companyId, userId, sessionId);
            return NoContent();
        }
    }
}
