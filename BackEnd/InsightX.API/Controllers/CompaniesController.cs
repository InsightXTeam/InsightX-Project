using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Extensions;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [ApiController]
    [Route("companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyCompany(CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _companyService.GetMyCompanyAsync(companyId, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut("setup")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Setup([FromBody] SetupDto dto, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _companyService.SetupAsync(dto, companyId, cancellationToken);
            return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, result.Error);
        }
    }
}
