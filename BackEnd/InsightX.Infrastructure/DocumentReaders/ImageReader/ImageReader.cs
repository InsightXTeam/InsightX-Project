using System.IO;
using InsightX.Application.Interfaces;

namespace InsightX.Infrastructure.DocumentReaders.Image
{
    public class ImageReader : IDocumentReader
    {
        private readonly IAIExtractionService _aiExtractionService;

        public ImageReader(IAIExtractionService aiExtractionService)
        {
            _aiExtractionService = aiExtractionService;
        }
        public bool CanRead(string extension)
        {
            return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            try
            {
                var imageBytes = await File.ReadAllBytesAsync(filePath);
                return await _aiExtractionService.ExtractTextFromImageAsync(imageBytes, filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"AI image text extraction error: {ex.Message}", ex);
            }
        }
    }
}