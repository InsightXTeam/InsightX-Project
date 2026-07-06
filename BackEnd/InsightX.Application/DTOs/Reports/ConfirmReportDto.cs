using System.Collections.Generic;

namespace InsightX.Application.DTOs.Reports
{
    public class ConfirmReportDto
    {
        public required List<ExtractedMetricDto> Metrics { get; set; }
    }
}
