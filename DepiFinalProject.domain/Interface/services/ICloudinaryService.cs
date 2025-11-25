using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DepiFinalProject.Core.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);

    }
}
