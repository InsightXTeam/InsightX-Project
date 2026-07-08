using InsightX.Application.Common;
using InsightX.Application.DTOs;
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
        private readonly IKpiService _kpiService;
        private readonly IAlertRepository _alertRepository;

        public ReportService(IReportRepository repository, IFileStorageService storage, IReportConfirmedHandler reportConfirmedHandler, IDeleteReportChunksUseCase deleteReportChunksUseCase, IKpiService kpiService, IAlertRepository alertRepository)
        {
            _repository = repository;
            _storage = storage;
            _reportConfirmedHandler = reportConfirmedHandler;
            _deleteReportChunksUseCase = deleteReportChunksUseCase;
            _kpiService = kpiService;
            _alertRepository = alertRepository;
        }

        public async Task<ServiceResult<ReportResponseDto>> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy, CancellationToken cancellationToken = default)
        {
            // Validate department is required
            if (!departmentId.HasValue)
                return ServiceResult<ReportResponseDto>.Fail(400, "A department must be selected for the report.");

            // Validate month/year
            if (dto.ReportMonth < 1 || dto.ReportMonth > 12)
                return ServiceResult<ReportResponseDto>.Fail(400, "Invalid report month. Must be between 1 and 12.");
            if (dto.ReportYear < 2000)
                return ServiceResult<ReportResponseDto>.Fail(400, "Invalid report year.");

            // Check for duplicate: one report per month per department
            var exists = await _repository.ExistsForMonthAsync(companyId, departmentId, dto.ReportMonth, dto.ReportYear, cancellationToken);
            if (exists)
                return ServiceResult<ReportResponseDto>.Fail(400, "A report has already been uploaded for this department in this month. Only one report per month per department is allowed.");

            var path = await _storage.SaveFileAsync(dto.File);
            var report = new Report
            {
                ReportName = string.IsNullOrWhiteSpace(dto.ReportName) ? dto.File.FileName : dto.ReportName,
                FileName = dto.File.FileName,
                FilePath = path,
                UploadedAt = DateTime.UtcNow,
                ReportMonth = dto.ReportMonth,
                ReportYear = dto.ReportYear,
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
                UploadedByName = "Me",
                ReportMonth = report.ReportMonth,
                ReportYear = report.ReportYear
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
                UploadedByName = x.UploadedBy?.Name ?? string.Empty,
                ReportMonth = x.ReportMonth,
                ReportYear = x.ReportYear
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
            
            var kpisResult = await _kpiService.GetAllAsync(companyId, cancellationToken);
            var departmentKpis = kpisResult.Data?
                .Where(k => k.DepartmentId == null || k.DepartmentId == report.DepartmentId)
                .ToList() ?? new List<KpiResponseDto>();

            var metrics = new List<ExtractedMetricDto>();
            foreach (var kpi in departmentKpis)
            {
                var matched = report.ExtractedMetrics
                    .Where(m => !string.IsNullOrWhiteSpace(m.KPIName) && m.KPIName.Equals(kpi.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Month)
                    .FirstOrDefault();

                if (matched != null && matched.Value.HasValue)
                {
                    metrics.Add(new ExtractedMetricDto
                    {
                        KPIName = kpi.Name,
                        Value = matched.Value,
                        Month = matched.Month,
                        Year = matched.Year
                    });
                }
                else
                {
                    metrics.Add(new ExtractedMetricDto
                    {
                        KPIName = kpi.Name,
                        Value = 0,
                        Month = report.UploadedAt.Month,
                        Year = report.UploadedAt.Year
                    });
                }
            }
            
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
                    var existingMetrics = report.ExtractedMetrics.Where(m => !string.IsNullOrWhiteSpace(m.KPIName) && m.KPIName.Equals(metricDto.KPIName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (existingMetrics.Any())
                    {
                        var primaryMetric = existingMetrics.First();
                        primaryMetric.Value = metricDto.Value ?? 0;
                        if (metricDto.Month.HasValue && metricDto.Month.Value >= 1 && metricDto.Month.Value <= 12)
                        {
                            primaryMetric.Month = metricDto.Month.Value;
                        }
                        if (metricDto.Year.HasValue && metricDto.Year.Value >= 2000)
                        {
                            primaryMetric.Year = metricDto.Year.Value;
                        }
                        primaryMetric.ConfirmedByManager = true;

                        foreach (var duplicate in existingMetrics.Skip(1))
                        {
                            report.ExtractedMetrics.Remove(duplicate);
                        }
                        
                        await _reportConfirmedHandler.HandleAsync(report.CompanyId, report.DepartmentId, metricDto.KPIName, (decimal)(metricDto.Value ?? 0), report.Id);
                    }
                    else
                    {
                        var newMetric = new ExtractedMetric
                        {
                            ReportId = report.Id,
                            CompanyId = report.CompanyId,
                            KPIName = metricDto.KPIName,
                            Value = metricDto.Value ?? 0,
                            Month = (metricDto.Month.HasValue && metricDto.Month.Value >= 1 && metricDto.Month.Value <= 12) ? metricDto.Month.Value : report.UploadedAt.Month,
                            Year = (metricDto.Year.HasValue && metricDto.Year.Value >= 2000) ? metricDto.Year.Value : report.UploadedAt.Year,
                            ConfirmedByManager = true
                        };
                        report.ExtractedMetrics.Add(newMetric);

                        await _reportConfirmedHandler.HandleAsync(report.CompanyId, report.DepartmentId, metricDto.KPIName, (decimal)(metricDto.Value ?? 0), report.Id);
                    }
                }
            }

            if (report.ExtractedMetrics != null)
            {
                foreach (var m in report.ExtractedMetrics)
                {
                    if (m.Month <= 0 || m.Month > 12) m.Month = report.UploadedAt.Month;
                    if (m.Year <= 0) m.Year = report.UploadedAt.Year;
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

            await _alertRepository.DeleteByReportIdAsync(id, cancellationToken);

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

        public async Task<ServiceResult<List<int>>> GetUploadedMonthsAsync(int companyId, int? departmentId, int year, string role, string userId, CancellationToken cancellationToken = default)
        {
            var months = await _repository.GetUploadedMonthsAsync(companyId, departmentId, year, role, userId, cancellationToken);
            return ServiceResult<List<int>>.Success(months);
        }
    }
}