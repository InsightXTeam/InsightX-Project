using InsightX.Application.Common;
using InsightX.Application.DTOs;
using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces.Rag;
using InsightX.Domain.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;

namespace InsightX.Application.UseCases.Documents
{
    public class DocumentProcessorService : IDocumentProcessor
    {
        private readonly IReportRepository _repository;
        private readonly IEnumerable<IDocumentReader> _readers;
        private readonly IAIExtractionService _ai;
        private readonly IKpiService _kpiService;
        private readonly IIndexReportUseCase _indexReportUseCase;
        private readonly ILogger<DocumentProcessorService> _logger;

        public DocumentProcessorService(IReportRepository repository, IEnumerable<IDocumentReader> readers, IAIExtractionService ai, IKpiService kpiService, IIndexReportUseCase indexReportUseCase, ILogger<DocumentProcessorService> logger)
        {
            _repository = repository;
            _readers = readers;
            _ai = ai;
            _kpiService = kpiService;
            _indexReportUseCase = indexReportUseCase;
            _logger = logger;
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

                var rawText = await reader.ExtractTextAsync(report.FilePath);

                // Clean the extracted text before sending to AI
                var text = CleanExtractedText(rawText);
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
                var departmentKpis = kpisResult.Data?
                    .Where(k => k.DepartmentId == null || k.DepartmentId == report.DepartmentId)
                    .ToList() ?? new List<KpiResponseDto>();

                if (!departmentKpis.Any())
                {
                    report.Status = ReportStatus.PendingConfirmation.ToString();
                    await _repository.UpdateAsync(report, cancellationToken);
                    return ServiceResult.Success(); // Treat as successful processing with 0 metrics
                }

                // Attempt AI extraction with retry on JSON parse failure
                List<ExtractedMetricDto>? extractedDtos = null;
                string? metricsJson = null;

                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    try
                    {
                        var reportDeptName = report.Department?.Name ?? departmentKpis.FirstOrDefault(k => k.DepartmentId == report.DepartmentId && !string.IsNullOrWhiteSpace(k.DepartmentName))?.DepartmentName;
                        metricsJson = await _ai.ExtractMetricsAsync(report.ExtractedText, departmentKpis, reportDeptName);

                        // Try to extract JSON array from the response
                        var jsonText = ExtractJsonArray(metricsJson);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                        };

                        extractedDtos = JsonSerializer.Deserialize<List<ExtractedMetricDto>>(jsonText, options);
                        break; // Success — exit retry loop
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning("AI JSON parse attempt {Attempt} failed. Raw response: {Response}. Error: {Error}",
                            attempt, metricsJson ?? "(null)", ex.Message);

                        if (attempt >= 2)
                        {
                            _logger.LogError("AI JSON parse failed after {Attempts} attempts for report {ReportId}", attempt, reportId);
                            // Continue with empty metrics rather than failing the entire report
                            extractedDtos = new List<ExtractedMetricDto>();
                        }
                        // Retry will use the same AI call which re-prompts naturally
                    }
                }

                // Remove any existing unconfirmed metrics if reprocessing
                report.ExtractedMetrics.Clear();

                var finalMetrics = new List<ExtractedMetric>();
                var extractedList = extractedDtos ?? new List<ExtractedMetricDto>();

                foreach (var kpiDto in departmentKpis)
                {
                    // Find matching extracted metric from AI (case-insensitive match)
                    var matched = extractedList
                        .Where(x => !string.IsNullOrWhiteSpace(x.KPIName) && x.KPIName.Equals(kpiDto.Name, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.Year ?? 0)
                        .ThenByDescending(x => x.Month ?? 0)
                        .FirstOrDefault();

                    if (matched != null && matched.Value.HasValue)
                    {
                        finalMetrics.Add(new ExtractedMetric
                        {
                            KPIName = kpiDto.Name,
                            Value = matched.Value.Value,
                            Month = (matched.Month.HasValue && matched.Month.Value >= 1 && matched.Month.Value <= 12) ? matched.Month.Value : report.UploadedAt.Month,
                            Year = (matched.Year.HasValue && matched.Year.Value >= 2000) ? matched.Year.Value : report.UploadedAt.Year,
                            CompanyId = report.CompanyId,
                            ConfirmedByManager = false
                        });
                    }
                    else
                    {
                        // If there is no value for specific KPI make it with its default value 0
                        finalMetrics.Add(new ExtractedMetric
                        {
                            KPIName = kpiDto.Name,
                            Value = 0,
                            Month = report.UploadedAt.Month,
                            Year = report.UploadedAt.Year,
                            CompanyId = report.CompanyId,
                            ConfirmedByManager = false
                        });
                    }
                }

                report.ExtractedMetrics = finalMetrics;

                report.Status = ReportStatus.PendingConfirmation.ToString();
                await _repository.UpdateAsync(report, cancellationToken);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process report {ReportId}: {Message}", reportId, ex.Message);
                report.Status = ReportStatus.Failed.ToString();
                await _repository.UpdateAsync(report, cancellationToken);
                return ServiceResult.Fail(500, ex.Message);
            }
        }

        /// <summary>
        /// Cleans extracted text by removing non-printable characters,
        /// collapsing excessive whitespace, and trimming empty lines.
        /// </summary>
        private static string CleanExtractedText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return string.Empty;

            // Remove non-printable characters (keep newlines, tabs, and standard whitespace)
            var cleaned = Regex.Replace(rawText, @"[^\x09\x0A\x0D\x20-\x7E\u00A0-\uFFFF]", "");

            // Collapse multiple consecutive blank lines into a single blank line
            cleaned = Regex.Replace(cleaned, @"(\r?\n\s*){3,}", "\n\n");

            // Collapse multiple consecutive spaces/tabs into a single space (within lines)
            cleaned = Regex.Replace(cleaned, @"[ \t]{2,}", " ");

            // Trim each line
            var lines = cleaned.Split('\n')
                .Select(line => line.TrimEnd())
                .ToArray();

            return string.Join("\n", lines).Trim();
        }

        /// <summary>
        /// Extracts a JSON array from potentially messy AI output that may contain
        /// markdown formatting, explanatory text, or other wrapper content.
        /// </summary>
        private static string ExtractJsonArray(string aiResponse)
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
                return "[]";

            // First try: find a JSON array with regex
            var match = Regex.Match(aiResponse, @"\[.*\]", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Value;
            }

            // Second try: strip markdown code fences
            var stripped = aiResponse
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            // Validate it looks like JSON
            if (stripped.StartsWith("["))
            {
                return stripped;
            }

            // Fallback: return empty array
            return "[]";
        }
    }
}