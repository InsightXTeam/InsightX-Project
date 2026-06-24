using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InsightX.Infrastructure.FileStorage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _folder = "Uploads";

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(_folder, fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return path;
        }
    }
}