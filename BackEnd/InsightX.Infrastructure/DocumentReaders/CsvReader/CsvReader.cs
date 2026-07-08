using InsightX.Application.Interfaces;

namespace InsightX.Infrastructure.DocumentReaders.CsvReader
{
    public class CsvReader : IDocumentReader
    {
        public bool CanRead(string extension)
        {
            return extension.Equals(".csv", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            // CSV and TXT files are already structured text — read as-is
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
