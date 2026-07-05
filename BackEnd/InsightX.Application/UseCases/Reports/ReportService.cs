using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightX.Domain.Enums;

namespace InsightX.Application.UseCases.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;
        private readonly IFileStorageService _storage;

        public ReportService(IReportRepository repository, IFileStorageService storage)
        {
            _repository = repository;
            _storage = storage;
        }

        public async Task<ReportResponseDto> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy)
        {
            var path = await _storage.SaveFileAsync(dto.File);
            var report = new Report
            {
                FileName = dto.File.FileName,
                FilePath = path,
                UploadedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending.ToString(),
                CompanyId = companyId,
                DepartmentId = departmentId,
                UploadedById = uploadedBy
            };
            await _repository.AddAsync(report);
            return new ReportResponseDto
            {
                Id = report.Id,
                FileName = report.FileName,
                Status = report.Status,
                UploadedAt = report.UploadedAt,
                UploadedById = report.UploadedById
            };
        }

        public async Task<List<ReportResponseDto>> GetReportsAsync(int companyId, string role, string userName)
        {
            var reports = await _repository.GetByCompanyAndUserAsync(companyId, role, userName);

            return reports.Select(x => new ReportResponseDto
            {
                Id = x.Id,
                FileName = x.FileName,
                Status = x.Status,
                UploadedAt = x.UploadedAt,
                UploadedById = x.UploadedById
            }).ToList();
        }

        public async Task<string> GetStatusAsync(int id, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId);
            if (report == null) throw new KeyNotFoundException("Report not found");
            return report.Status;
        }

        public async Task<string> GetPreviewAsync(int id, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId);
            if (report == null || report.ExtractedMetrics == null) throw new KeyNotFoundException("Report not found");
            return System.Text.Json.JsonSerializer.Serialize(report.ExtractedMetrics.Select(m => new ExtractedMetricDto
            {
                KPIName = m.KPIName,
                Value = m.Value,
                Month = m.Month,
                Year = m.Year
            }));
        }

        public async Task<string> GetExtractedTextAsync(int id, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId);
            if (report == null) throw new KeyNotFoundException("Report not found");
            return report.ExtractedText;
        }

        public async Task ConfirmTextAsync(int id, ConfirmReportDto dto, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId);
            if (report == null) throw new KeyNotFoundException("Report not found");

            if (report.Status != ReportStatus.PendingConfirmation.ToString())
                throw new Exception("Report is not pending confirmation");

            report.ExtractedText = dto.ExtractedText;
            await _repository.UpdateAsync(report);
        }

        public async Task DeleteAsync(int id, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId);
            if (report == null) throw new KeyNotFoundException("Report not found");

            _storage.DeleteFile(report.FilePath);
            await _repository.DeleteAsync(report);
        }
    }
}