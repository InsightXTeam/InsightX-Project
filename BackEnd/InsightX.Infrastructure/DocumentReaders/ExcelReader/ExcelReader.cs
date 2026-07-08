using ExcelDataReader;
using InsightX.Application.Interfaces;
using System.Text;

namespace InsightX.Infrastructure.DocumentReaders
{
    public class ExcelReader : IDocumentReader
    {
        public bool CanRead(string extension)
        {
            return extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".xls", StringComparison.OrdinalIgnoreCase);
            //     ↑ Now supports both formats
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                // ExcelReaderFactory auto-detects .xls vs .xlsx

                var sb = new StringBuilder();
                int sheetIndex = 0;

                do
                {
                    sheetIndex++;
                    var sheetName = reader.Name ?? $"Sheet{sheetIndex}";
                    sb.AppendLine($"=== Sheet: {sheetName} ===");

                    bool isFirstRow = true;
                    while (reader.Read())
                    {
                        var cells = new List<string>();
                        for (int col = 0; col < reader.FieldCount; col++)
                        {
                            cells.Add(reader.GetValue(col)?.ToString()?.Trim() ?? "");
                        }

                        var row = string.Join(" | ", cells);

                        if (isFirstRow)
                        {
                            // Mark the first row as a header row for AI context
                            sb.AppendLine($"[Header] {row}");
                            sb.AppendLine(new string('-', Math.Min(row.Length, 80)));
                            isFirstRow = false;
                        }
                        else
                        {
                            sb.AppendLine(row);
                        }
                    }

                    sb.AppendLine(); // Blank line between sheets
                } while (reader.NextResult()); // Handles multiple sheets

                return sb.ToString();
            });
        }
    }
}