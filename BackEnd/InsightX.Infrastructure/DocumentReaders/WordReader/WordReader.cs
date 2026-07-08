using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using InsightX.Application.Interfaces;
using System.Text;

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

                var sb = new StringBuilder();

                // Iterate through all child elements to preserve document order
                foreach (var element in body.ChildElements)
                {
                    if (element is Paragraph paragraph)
                    {
                        var text = paragraph.InnerText?.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            sb.AppendLine(text);
                        }
                    }
                    else if (element is Table table)
                    {
                        // Extract table data with pipe-delimited formatting
                        sb.AppendLine("[Table]");
                        bool isFirstRow = true;
                        foreach (var row in table.Elements<TableRow>())
                        {
                            var cells = row.Elements<TableCell>()
                                .Select(c => c.InnerText?.Trim() ?? "")
                                .ToList();
                            var rowText = string.Join(" | ", cells);

                            if (isFirstRow)
                            {
                                sb.AppendLine($"[Header] {rowText}");
                                sb.AppendLine(new string('-', Math.Min(rowText.Length, 80)));
                                isFirstRow = false;
                            }
                            else
                            {
                                sb.AppendLine(rowText);
                            }
                        }
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            });
        }
    }
}