using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);

    }
}
