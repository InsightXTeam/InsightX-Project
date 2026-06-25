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

            var text = await reader.ExtractTextAsync(report.FilePath);

            var kpis = await _kpiRepository.GetKpiNamesByCompanyIdAsync(report.CompanyId);
            if (kpis == null || !kpis.Any())
            {
                kpis = new List<string> { "Revenue" }; // fallback just in case
            }

            var metricsJson = await _ai.ExtractMetricsAsync(text, kpis);

            Console.WriteLine(metricsJson);

            metricsJson = metricsJson.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            var extractedDtos = JsonSerializer.Deserialize<List<ExtractedMetricDto>>(metricsJson, options);

            report.ExtractedText = text;
            report.Status = "Pending Confirmation";

            if (extractedDtos != null && extractedDtos.Any())
            {
                report.ExtractedMetrics = extractedDtos.Select(x => new ExtractedMetric
                {
                    KPIName = x.KPIName,
                    Value = x.Value,
                    //Month = (x.Month == null || x.Month == 0) ? DateTime.Now.Month : x.Month.Value,
                    Month = DateTime.Now.Month,
                    //Year = (x.Year == null || x.Year == 0) ? DateTime.Now.Year : x.Year.Value,
                    Year = DateTime.Now.Year,
                    CompanyId = report.CompanyId,
                    ConfirmedByManager = false
                }).ToList();
            }

            await _repository.UpdateAsync(report);
        }
    }
}