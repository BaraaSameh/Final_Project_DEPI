using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<ICollection<User>> GetAllAsync();//new
        Task<bool> ChangePasswordAsync(User updateduser);//new
        Task<User> GetByIdAsync(int userId);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByPhoneAsync(string phone);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);
    }
}
