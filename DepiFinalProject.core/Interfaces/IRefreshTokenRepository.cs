using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
        Task RevokeAllUserTokensAsync(int userId);
        Task DeleteExpiredTokensAsync();
    }
}
