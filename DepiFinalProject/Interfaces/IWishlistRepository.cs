using DepiFinalProject.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Interfaces
{
    public interface IWishlistRepository
    {
        Task<List<WishlistItemDto>> GetAllAsync(int userId);
        Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId);
        Task AddAsync(int userId, WishlistItemDto item);
        Task RemoveAsync(int userId, int productId);
        Task ClearAsync(int userId);
    }
}
