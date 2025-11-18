using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using DepiFinalProject.Interfaces;

namespace DepiFinalProject.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var acc = new Account(
                options.Value.CloudName,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<(string Url, string PublicId)> UploadUserImageAsync(IFormFile file)
        {
            const long maxfilesize = 1024 * 1024;
            if (file == null || file.Length == 0)
                throw new BadHttpRequestException("Image is required");

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                throw new BadHttpRequestException("Only JPG, PNG, WEBP files are allowed");
            if(file.Length>maxfilesize)
                throw new BadHttpRequestException("image size shouldn't be more than 1 MB");
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ecommerce/users"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Cloudinary upload failed");

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            var deletionParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }
    }
}
