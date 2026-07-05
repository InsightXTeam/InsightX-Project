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
            // Required for ExcelDataReader to work on non-Windows or .NET Core
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return await Task.Run(() =>
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                // ExcelReaderFactory auto-detects .xls vs .xlsx

                var sb = new StringBuilder();

                do
                {
                    while (reader.Read())
                    {
                        for (int col = 0; col < reader.FieldCount; col++)
                        {
                            sb.Append(reader.GetValue(col)?.ToString() ?? "");
                            sb.Append(' ');
                        }
                        sb.AppendLine();
                    }
                } while (reader.NextResult()); // Handles multiple sheets

                return sb.ToString();
            });
        }
    }
}