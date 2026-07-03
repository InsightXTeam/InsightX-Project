using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("rag")]
    public class RagController : ControllerBase
    {
        private readonly IRagRetrievalService _ragRetrievalService;
        private readonly ICurrentUserService _currentUserService;

        public RagController(
            IRagRetrievalService ragRetrievalService,
            ICurrentUserService currentUserService)
        {
            _ragRetrievalService = ragRetrievalService;
            _currentUserService = currentUserService;
        }

        [HttpPost("retrieve")]
        public async Task<IActionResult> Retrieve([FromBody] RagRetrieveRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("Question is required.");
            }

            var chunks = await _ragRetrievalService.RetrieveAsync(
                request.Question,
                _currentUserService.CompanyId,
                request.Limit);

            return Ok(chunks);
        }
    }
}
