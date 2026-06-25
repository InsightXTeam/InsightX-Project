using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using InsightX.Domain.Entities.Reports;
using System.Text.Json;

namespace InsightX.Application.UseCases.Documents
{
    public class DocumentProcessorService : IDocumentProcessor
    {
        private readonly IReportRepository _repository;
        private readonly IEnumerable<IDocumentReader> _readers;
        private readonly IAIExtractionService _ai;
        private readonly IKPIRepository _kpiRepository;

        public DocumentProcessorService(IReportRepository repository, IEnumerable<IDocumentReader> readers, IAIExtractionService ai, IKPIRepository kpiRepository)
        {
            _repository = repository;
            _readers = readers;
            _ai = ai;
            _kpiRepository = kpiRepository;
        }

        public async Task ProcessAsync(int reportId)
        {
            var report = await _repository.GetByIdAsync(reportId);

            if (report == null)
                throw new Exception("Report not found");

            report.Status = "Processing";
            await _repository.UpdateAsync(report);

            var extension = Path.GetExtension(report.FilePath);

            var reader = _readers.FirstOrDefault(x => x.CanRead(extension));

            if (reader == null)
            {
                report.Status = "Failed";
                await _repository.UpdateAsync(report);
                throw new Exception("No reader found");
            }

            // ONLY extract text here
            var text = await reader.ExtractTextAsync(report.FilePath);

            report.ExtractedText = text;
            report.Status = "Pending Confirmation"; // Waiting for user to review the text

            await _repository.UpdateAsync(report);
        }

        public async Task ExtractKpisAsync(int reportId)
        {
            var report = await _repository.GetByIdAsync(reportId);

            if (report == null)
                throw new Exception("Report not found");

            if (string.IsNullOrEmpty(report.ExtractedText))
                throw new Exception("No text available to extract KPIs from");

            report.Status = "Processing AI";
            await _repository.UpdateAsync(report);

            try
            {
                var kpis = await _kpiRepository.GetKpiNamesByCompanyIdAsync(report.CompanyId);
                if (kpis == null || !kpis.Any())
                {
                    kpis = new List<string> { "Revenue" }; // fallback just in case
                }

                // AI runs on the MANAGER CONFIRMED text
                var metricsJson = await _ai.ExtractMetricsAsync(report.ExtractedText, kpis);

                Console.WriteLine(metricsJson);

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
                        Month = DateTime.Now.Month,
                        Year = DateTime.Now.Year,
                        CompanyId = report.CompanyId,
                        ConfirmedByManager = true // Implicitly confirmed since manager verified the text
                    }).ToList();
                }

                report.Status = "Done";
                await _repository.UpdateAsync(report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting KPIs: {ex.Message}");
                report.Status = "Failed AI";
                await _repository.UpdateAsync(report);
                throw;
            }
        }
    }
}