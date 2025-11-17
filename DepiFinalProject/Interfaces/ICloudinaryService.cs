using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadUserImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);

    }
}
