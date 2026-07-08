using InsightX.Application.Interfaces;
using iTextSharp.text.pdf.parser;
using System.Text;

namespace InsightX.Infrastructure.DocumentReaders
{
    public class PdfDocumentReader : IDocumentReader
    {
        public bool CanRead(string extension)
        {
            return extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();

                using (var pdfReader = new iTextSharp.text.pdf.PdfReader(filePath))
                {
                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        if (page > 1)
                            sb.AppendLine();

                        sb.AppendLine($"--- Page {page} of {pdfReader.NumberOfPages} ---");

                        // Use SimpleTextExtractionStrategy for better layout preservation
                        var strategy = new SimpleTextExtractionStrategy();
                        var pageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                        sb.AppendLine(pageText);
                    }
                }

                return sb.ToString();
            });
        }
    }
}