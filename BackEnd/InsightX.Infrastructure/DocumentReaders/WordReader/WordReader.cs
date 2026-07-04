using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using InsightX.Application.Interfaces;

namespace InsightX.Infrastructure.DocumentReaders.WordReader
{
    public class WordReader : IDocumentReader
    {
        public bool CanRead(string extension)
        {
            return extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var document = WordprocessingDocument.Open(filePath, false);

                var body = document?.MainDocumentPart?.Document?.Body;
                if (body == null)
                {
                    throw new InvalidOperationException($"Invalid Word document structure. No body found.{filePath}");
                }
                var text = string.Join(Environment.NewLine, body.Descendants<Paragraph>().Select(p => p.InnerText));

                return text;
            });
        }
    }
}