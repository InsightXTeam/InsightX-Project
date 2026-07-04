using InsightX.Application.Interfaces;
using Tesseract;

namespace InsightX.Infrastructure.DocumentReaders.Image
{
    public class ImageReader : IDocumentReader
    {
        public bool CanRead(string extension)
        {
            return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // مسار ملفات اللغات
                    string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

                    //EngineMode.LstmOnly افضل
                    //EngineMode.Default

                    using var engine = new TesseractEngine(tessPath, "eng+ara", EngineMode.Default);
                    //using var engine = new TesseractEngine(tessPath, "eng+ara", EngineMode.LstmOnly);

                    using var img = Pix.LoadFromFile(filePath);

                    using var page = engine.Process(img);

                    string text = page.GetText();

                    return text?.Trim() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"OCR Error: {ex.Message}");
                }
            });
        }
    }
}