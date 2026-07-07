using InsightX.Application.Common;
using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightX.Domain.Enums;
using InsightXAI.Application.Interfaces.Rag;
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
        private readonly IReportConfirmedHandler _reportConfirmedHandler;
        private readonly IDeleteReportChunksUseCase _deleteReportChunksUseCase;

        public ReportService(IReportRepository repository, IFileStorageService storage, IReportConfirmedHandler reportConfirmedHandler, IDeleteReportChunksUseCase deleteReportChunksUseCase)
        {
            _repository = repository;
            _storage = storage;
            _reportConfirmedHandler = reportConfirmedHandler;
            _deleteReportChunksUseCase = deleteReportChunksUseCase;
        }

        public async Task<ServiceResult<ReportResponseDto>> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy, CancellationToken cancellationToken = default)
        {
            var path = await _storage.SaveFileAsync(dto.File);
            var report = new Report
            {
                ReportName = string.IsNullOrWhiteSpace(dto.ReportName) ? dto.File.FileName : dto.ReportName,
                FileName = dto.File.FileName,
                FilePath = path,
                UploadedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending.ToString(),
                CompanyId = companyId,
                DepartmentId = departmentId,
                UploadedById = uploadedBy
            };
            try 
            {
                await _repository.AddAsync(report, cancellationToken);
            } 
            catch (Exception ex) 
            {
                return ServiceResult<ReportResponseDto>.Fail(500, $"DB Error: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }

            return ServiceResult<ReportResponseDto>.Success(new ReportResponseDto
            {
                Id = report.Id,
                ReportName = report.ReportName,
                FileName = report.FileName,
                Status = report.Status,
                UploadedAt = report.UploadedAt,
                UploadedById = report.UploadedById,
                UploadedByName = "Me" // Temporary until list reload
            });
        }

        public async Task<ServiceResult<List<ReportResponseDto>>> GetReportsAsync(int companyId, int? departmentId, string role, string userName, CancellationToken cancellationToken = default)
        {
            var reports = await _repository.GetByCompanyAndUserAsync(companyId, departmentId, role, userName, cancellationToken);

            return ServiceResult<List<ReportResponseDto>>.Success(reports.Select(x => new ReportResponseDto
            {
                Id = x.Id,
                ReportName = x.ReportName,
                FileName = x.FileName,
                Status = x.Status,
                UploadedAt = x.UploadedAt,
                UploadedById = x.UploadedById,
                UploadedByName = x.UploadedBy?.Name ?? string.Empty
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

        public async Task<ServiceResult> ConfirmTextAsync(int id, ConfirmReportDto dto, int companyId, string role, string userName, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null) return ServiceResult.Fail(404, "Report not found");
            
            if (role != "Owner" && report.UploadedById != userName) return ServiceResult.Fail(403, "You do not have permission to confirm this report.");

            if (report.Status != ReportStatus.PendingConfirmation.ToString())
                return ServiceResult.Fail(400, "Report is not pending confirmation");

            if (report.ExtractedMetrics != null && dto.Metrics != null)
            {
                foreach (var metricDto in dto.Metrics)
                {
                    var existingMetric = report.ExtractedMetrics.FirstOrDefault(m => m.KPIName == metricDto.KPIName);
                    if (existingMetric != null)
                    {
                        existingMetric.Value = metricDto.Value;
                        existingMetric.ConfirmedByManager = true;
                        
                        await _reportConfirmedHandler.HandleAsync(report.CompanyId, report.DepartmentId, metricDto.KPIName, (decimal)(metricDto.Value ?? 0));
                    }
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

            // Delete from Vector DB first before the relational DB record is removed
            await _deleteReportChunksUseCase.ExecuteAsync(companyId, id, userName, role, cancellationToken);

            await _repository.DeleteAsync(report, cancellationToken);
            _storage.DeleteFile(report.FilePath);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<ReportDownloadDto>> DownloadAsync(int id, int companyId, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(id, companyId, cancellationToken);
            if (report == null) return ServiceResult<ReportDownloadDto>.Fail(404, "Report not found");

            var fileContent = await _storage.GetFileAsync(report.FilePath);
            if (fileContent == null || fileContent.Length == 0) return ServiceResult<ReportDownloadDto>.Fail(404, "File not found on disk");

            string ext = Path.GetExtension(report.FileName).ToLowerInvariant();
            string contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".csv" => "text/csv",
                ".txt" => "text/plain",
                _ => "application/octet-stream",
            };

            var dto = new ReportDownloadDto
            {
                FileContent = fileContent,
                ContentType = contentType,
                FileName = report.FileName
            };

            return ServiceResult<ReportDownloadDto>.Success(dto);
        }
    }
}