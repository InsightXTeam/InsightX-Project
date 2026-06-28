namespace InsightX.Application.Interfaces
{
    public interface IDocumentReader
    {
        bool CanRead(string extension);

        Task<string> ExtractTextAsync(string filePath);
    }
}