using InsightX.Application.Interfaces;
using OfficeOpenXml;

namespace InsightX.Infrastructure.DocumentReaders
{
    public class ExcelReader : IDocumentReader
    {
        public bool CanRead(string extension)
        {
            return extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Mohamed");

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage(new FileInfo(filePath));

                var sheet = package.Workbook.Worksheets[0];

                var text = string.Empty;

                for (int row = 1; row <= sheet.Dimension.Rows; row++)
                {
                    for (int col = 1; col <= sheet.Dimension.Columns; col++)
                    {
                        text += sheet.Cells[row, col].Text + " ";
                    }
                    text += Environment.NewLine;
                }
                return text;
            });
        }
    }
}