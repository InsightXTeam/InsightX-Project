using InsightX.Application.Common;
using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightX.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<ServiceResult<ReportResponseDto>> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy, CancellationToken cancellationToken = default)
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
            await _repository.AddAsync(report, cancellationToken);
            return ServiceResult<ReportResponseDto>.Success(new ReportResponseDto
            {
                Id = report.Id,
                FileName = report.FileName,
                Status = report.Status,
                UploadedAt = report.UploadedAt,
                UploadedById = report.UploadedById
            });
        }

        public async Task<ServiceResult<List<ReportResponseDto>>> GetReportsAsync(int companyId, string role, string userName, CancellationToken cancellationToken = default)
        {
            var reports = await _repository.GetByCompanyAndUserAsync(companyId, role, userName, cancellationToken);

            return ServiceResult<List<ReportResponseDto>>.Success(reports.Select(x => new ReportResponseDto
            {
                Id = x.Id,
                FileName = x.FileName,
                Status = x.Status,
                UploadedAt = x.UploadedAt,
                UploadedById = x.UploadedById
            }).ToList());
        }

        public async Task<ServiceResult<string>> GetStatusAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null) return ServiceResult<string>.Fail(404, "Report not found");
            return ServiceResult<string>.Success(report.Status);
        }

        public async Task<ServiceResult<List<ExtractedMetricDto>>> GetPreviewAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null || report.ExtractedMetrics == null) return ServiceResult<List<ExtractedMetricDto>>.Fail(404, "Report not found");
            
            var metrics = report.ExtractedMetrics.Select(m => new ExtractedMetricDto
            {
                KPIName = m.KPIName,
                Value = m.Value,
                Month = m.Month,
                Year = m.Year
            }).ToList();
            
            return ServiceResult<List<ExtractedMetricDto>>.Success(metrics);
        }

        public async Task<ServiceResult<string>> GetExtractedTextAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null) return ServiceResult<string>.Fail(404, "Report not found");
            return ServiceResult<string>.Success(report.ExtractedText);
        }

        public async Task<ServiceResult> ConfirmMetricsAsync(int id, ConfirmReportDto dto, int companyId, string role, string userName, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null) return ServiceResult.Fail(404, "Report not found");
            
            if (role != "Owner" && report.UploadedById != userName) return ServiceResult.Fail(403, "You do not have permission to confirm this report.");

            if (report.Status != ReportStatus.PendingConfirmation.ToString())
                return ServiceResult.Fail(400, "Report is not pending confirmation");

            // Clear old unconfirmed metrics
            report.ExtractedMetrics.Clear();

            // Add new confirmed metrics
            if (dto.Metrics != null)
            {
                foreach (var m in dto.Metrics)
                {
                    report.ExtractedMetrics.Add(new ExtractedMetric
                    {
                        KPIName = m.KPIName,
                        Value = m.Value,
                        Month = m.Month ?? report.UploadedAt.Month,
                        Year = m.Year ?? report.UploadedAt.Year,
                        CompanyId = report.CompanyId,
                        ConfirmedByManager = true
                    });
                }
            }

            report.Status = ReportStatus.Done.ToString();
            await _repository.UpdateAsync(report, cancellationToken);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id, int companyId, string role, string userName, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null) return ServiceResult.Fail(404, "Report not found");
            
            if (role != "Owner" && report.UploadedById != userName) return ServiceResult.Fail(403, "You do not have permission to delete this report.");

            await _repository.DeleteAsync(report, cancellationToken);
            _storage.DeleteFile(report.FilePath);
            return ServiceResult.Success();
        }
    }
}