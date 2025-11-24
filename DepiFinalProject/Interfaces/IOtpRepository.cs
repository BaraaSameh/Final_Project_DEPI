using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IOtpRepository
    {
        Task AddAsync(OTP entry);
        Task<OTP> GetValidOtpAsync(int userId, string purpose, string codeHash = null);
        Task<OTP> GetByIdAsync(int id);
        Task SaveChangesAsync();

    }
}
