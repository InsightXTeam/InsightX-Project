using InsightX.Application.Interfaces;
using OfficeOpenXml;
using System.Text;

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
            ExcelPackage.License.SetNonCommercialPersonal("InsightX");

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage(new FileInfo(filePath));

                var sheet = package.Workbook.Worksheets[0];

                var text = new StringBuilder();

                for (int row = 1; row <= sheet.Dimension.Rows; row++)
                {
                    for (int col = 1; col <= sheet.Dimension.Columns; col++)
                    {
                        text.Append(sheet.Cells[row, col].Text + " ");
                    }
                    text.Append(Environment.NewLine);
                }
                return text.ToString();
            });
        }
    }
}