using Microsoft.AspNetCore.Http;

namespace InsightX.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file);
        void DeleteFile(string filePath);
    }
}