using InsightX.Application.Interfaces;
using iTextSharp.text.pdf.parser;

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
                var text = string.Empty;

                using (var pdfReader = new iTextSharp.text.pdf.PdfReader(filePath))
                {
                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        text += PdfTextExtractor.GetTextFromPage(pdfReader, page);
                    }
                }

                return text;
            });
        }
    }
}