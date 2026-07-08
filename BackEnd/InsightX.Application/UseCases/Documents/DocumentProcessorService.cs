using InsightX.Application.Common;
using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces.Rag;
using InsightX.Domain.Enums;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace InsightX.Application.UseCases.Documents
{
    public class DocumentProcessorService : IDocumentProcessor
    {
        private readonly IReportRepository _repository;
        private readonly IEnumerable<IDocumentReader> _readers;
        private readonly IAIExtractionService _ai;
        private readonly IKpiService _kpiService;
        private readonly IIndexReportUseCase _indexReportUseCase;

        public DocumentProcessorService(IReportRepository repository, IEnumerable<IDocumentReader> readers, IAIExtractionService ai, IKpiService kpiService, IIndexReportUseCase indexReportUseCase)
        {
            _repository = repository;
            _readers = readers;
            _ai = ai;
            _kpiService = kpiService;
            _indexReportUseCase = indexReportUseCase;
        }

        public async Task<ServiceResult> ProcessAsync(int reportId, int companyId, string role, string userName, CancellationToken cancellationToken = default)
        {
            var report = await _repository.GetByIdForCompanyAsync(reportId, companyId, cancellationToken);

            if (report == null)
                return ServiceResult.Fail(404, "Report not found");
                
            if (role != "Owner" && report.UploadedById != userName)
                return ServiceResult.Fail(403, "You do not have permission to process this report.");

            report.Status = ReportStatus.Processing.ToString();
            await _repository.UpdateAsync(report, cancellationToken);

            try
            {
                var extension = Path.GetExtension(report.FilePath);
                var reader = _readers.FirstOrDefault(x => x.CanRead(extension));

                if (reader == null)
                {
                    report.Status = ReportStatus.Failed.ToString();
                    await _repository.UpdateAsync(report, cancellationToken);
                    return ServiceResult.Fail(400, "No reader found for this file type");
                }

                var text = await reader.ExtractTextAsync(report.FilePath);
                report.ExtractedText = text;

                report.Status = ReportStatus.ProcessingAI.ToString();
                await _repository.UpdateAsync(report, cancellationToken);

                var indexRequest = new IndexReportRequestDto
                {
                    ReportId = report.Id,
                    Month = report.UploadedAt.ToString("MMMM"),
                    Year = report.UploadedAt.Year,
                    FullText = text
                };
                await _indexReportUseCase.ExecuteAsync(indexRequest, companyId, report.DepartmentId, userName, role, cancellationToken);

                var kpisResult = await _kpiService.GetAllAsync(companyId);
                var kpis = kpisResult.Data?.Select(k => k.Name).ToList() ?? new List<string>();
                if (!kpis.Any())
                {
                    report.Status = ReportStatus.PendingConfirmation.ToString();
                    await _repository.UpdateAsync(report, cancellationToken);
                    return ServiceResult.Success(); // Treat as successful processing with 0 metrics
                }

                var metricsJson = await _ai.ExtractMetricsAsync(report.ExtractedText, kpis);

                var match = System.Text.RegularExpressions.Regex.Match(metricsJson, @"\[.*\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                if (match.Success)
                {
                    metricsJson = match.Value;
                }
                else
                {
                    metricsJson = metricsJson.Replace("```json", "").Replace("```", "").Trim();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var extractedDtos = JsonSerializer.Deserialize<List<ExtractedMetricDto>>(metricsJson, options);

                if (extractedDtos != null && extractedDtos.Any())
                {
                    // Remove any existing unconfirmed metrics if reprocessing
                    report.ExtractedMetrics.Clear();

                    report.ExtractedMetrics = extractedDtos.Select(x => new ExtractedMetric
                    {
                        KPIName = x.KPIName,
                        Value = x.Value,
                        Month = (x.Month.HasValue && x.Month.Value >= 1 && x.Month.Value <= 12) ? x.Month.Value : report.UploadedAt.Month,
                        Year = (x.Year.HasValue && x.Year.Value >= 2000) ? x.Year.Value : report.UploadedAt.Year,
                        CompanyId = report.CompanyId,
                        ConfirmedByManager = false
                    }).ToList();
                }

                report.Status = ReportStatus.PendingConfirmation.ToString();
                await _repository.UpdateAsync(report, cancellationToken);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                report.Status = ReportStatus.Failed.ToString();
                await _repository.UpdateAsync(report, cancellationToken);
                return ServiceResult.Fail(500, ex.Message);
            }
        }
    }
}