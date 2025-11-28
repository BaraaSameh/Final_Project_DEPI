using DepiFinalProject.core.DTOs;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IUserService
    {
        Task<ICollection<UserResponseDTO>> GetAllAsync();//new
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);//new
        Task<UserResponseDTO> GetByIdAsync(int userId);
        Task<User> GetByEmailAsync(string email);
        Task<UserResponseDTO> GetByEmailForApiAsync(string email); // Add this for API endpoints
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateAsync(CreateUserDTO user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);
        Task<string> UpdateUserImageAsync(int userId, IFormFile file);
        Task<bool> DeleteUserImageAsync(int userId);
        Task RequestEmailVerificationOtpAsync(string userEmail);
        Task<bool> VerifyEmailOtpAsync(User user, string otpCode);
    }
}
