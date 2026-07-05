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
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _departmentService.CreateAsync(dto, companyId, cancellationToken);
            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _departmentService.GetByIdAsync(id, companyId, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _departmentService.GetAllAsync(companyId, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _departmentService.UpdateAsync(id, dto, companyId, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var result = await _departmentService.DeleteAsync(id, companyId, cancellationToken);
            return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
        }
    }
}
