using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using InsightX.Domain.Enums;
using System.Text.Json;

namespace InsightX.Application.UseCases.Documents
{
    public class DocumentProcessorService : IDocumentProcessor
    {
        private readonly IReportRepository _repository;
        private readonly IEnumerable<IDocumentReader> _readers;
        private readonly IAIExtractionService _ai;

        public DocumentProcessorService(IReportRepository repository, IEnumerable<IDocumentReader> readers, IAIExtractionService ai)
        {
            _repository = repository;
            _readers = readers;
            _ai = ai;
        }

        public async Task ProcessAsync(int reportId, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(reportId, companyId);

            if (report == null)
                throw new KeyNotFoundException("Report not found");

            report.Status = ReportStatus.Processing.ToString();
            await _repository.UpdateAsync(report);

            try
            {
                var extension = Path.GetExtension(report.FilePath);
                var reader = _readers.FirstOrDefault(x => x.CanRead(extension));

                if (reader == null)
                    throw new Exception("No reader found for this file type");

                var text = await reader.ExtractTextAsync(report.FilePath);

                report.ExtractedText = text;
                report.Status = ReportStatus.PendingConfirmation.ToString();
                await _repository.UpdateAsync(report);
            }
            catch (Exception)
            {
                report.Status = ReportStatus.Failed.ToString();
                await _repository.UpdateAsync(report);
                throw;
            }
        }

        public async Task ExtractKpisAsync(int reportId, int companyId)
        {
            var report = await _repository.GetByIdForCompanyAsync(reportId, companyId);

            if (report == null)
                throw new KeyNotFoundException("Report not found");

            if (string.IsNullOrEmpty(report.ExtractedText))
                throw new Exception("No text available to extract KPIs from");

            report.Status = ReportStatus.ProcessingAI.ToString();
            await _repository.UpdateAsync(report);

            try
            {
                var kpis = new List<string> { "Revenue" };
                var metricsJson = await _ai.ExtractMetricsAsync(report.ExtractedText, kpis);

                metricsJson = metricsJson.Replace("```json", "").Replace("```", "").Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var extractedDtos = JsonSerializer.Deserialize<List<ExtractedMetricDto>>(metricsJson, options);

                if (extractedDtos != null && extractedDtos.Any())
                {
                    report.ExtractedMetrics = extractedDtos.Select(x => new ExtractedMetric
                    {
                        KPIName = x.KPIName,
                        Value = x.Value,
                        Month = x.Month ?? report.UploadedAt.Month,
                        Year = x.Year ?? report.UploadedAt.Year,
                        CompanyId = report.CompanyId,
                        ConfirmedByManager = true
                    }).ToList();
                }

                report.Status = ReportStatus.Done.ToString();
                await _repository.UpdateAsync(report);
            }
            catch (Exception)
            {
                report.Status = ReportStatus.FailedAI.ToString();
                await _repository.UpdateAsync(report);
                throw;
            }
        }
    }
}