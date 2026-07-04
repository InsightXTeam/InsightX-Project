using System.Threading.Tasks;
using InsightX.Application.DTOs;
using InsightX.Application.Extensions;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [ApiController]
    [Route("kpis")]
    public class KpisController : ControllerBase
    {
        private readonly IKpiService _kpiService;

        public KpisController(IKpiService kpiService)
        {
            _kpiService = kpiService;
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Create([FromBody] CreateKpiDto dto)
        {
            var companyId = User.GetCompanyId();
            var result = await _kpiService.CreateAsync(dto, companyId);
            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var companyId = User.GetCompanyId();
            var result = await _kpiService.GetByIdAsync(id, companyId);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var companyId = User.GetCompanyId();
            var result = await _kpiService.GetAllAsync(companyId);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateKpiDto dto)
        {
            var companyId = User.GetCompanyId();
            var result = await _kpiService.UpdateAsync(id, dto, companyId);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = User.GetCompanyId();
            var result = await _kpiService.DeleteAsync(id, companyId);
            return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
        }
    }
}
