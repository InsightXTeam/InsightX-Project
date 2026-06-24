using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;

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
                UploadedAt = DateTime.Now,
                Status = "Pending",
                CompanyId = companyId,
                DepartmentId = departmentId ?? 0,
                UploadedBy = uploadedBy
            };
            await _repository.AddAsync(report);
            return new ReportResponseDto
            {
                Id = report.Id,
                FileName = report.FileName,
                Status = report.Status,
                UploadedAt = report.UploadedAt
            };
        }

        public async Task<List<ReportResponseDto>> GetReportsAsync()
        {
            var reports = await _repository.GetAllAsync();
            return reports.Select(x => new ReportResponseDto
            {
                Id = x.Id,
                FileName = x.FileName,
                Status = x.Status,
                UploadedAt = x.UploadedAt
            }).ToList();
        }

        public async Task<string> GetStatusAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            return report?.Status ?? "Not Found";
        }

        public async Task<string> GetPreviewAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null || report.ExtractedMetrics == null) return "Not Found";
            return System.Text.Json.JsonSerializer.Serialize(report.ExtractedMetrics.Select(m => new ExtractedMetricDto
            {
                KPIName = m.KPIName,
                Value = m.Value,
                Month = m.Month,
                Year = m.Year
            }));
        }

        public async Task<string> GetExtractedTextAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            return report?.ExtractedText ?? "Not Found";
        }

        public async Task ConfirmAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null) throw new Exception("Report not found");

            if (report.Status != "Pending Confirmation")
                throw new Exception("Report is not pending confirmation");

            foreach (var metric in report.ExtractedMetrics)
            {
                metric.ConfirmedByManager = true;
            }

            report.Status = "Done";
            await _repository.UpdateAsync(report);
        }

        public async Task DeleteAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report != null)
            {
                await _repository.DeleteAsync(report);
            }
        }
    }
}