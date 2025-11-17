using DepiFinalProject.DTOs;
using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IUserService
    {
        Task<ICollection<UserResponseDTO>> GetAllAsync();//new
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);//new
        Task<UserResponseDTO> GetByIdAsync(int userId);
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);
        Task<string> UpdateUserImageAsync(int userId, IFormFile file);
        Task<bool> DeleteUserImageAsync(int userId);
    }
}
