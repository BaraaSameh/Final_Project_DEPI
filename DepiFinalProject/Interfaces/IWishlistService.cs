using DepiFinalProject.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetAllAsync(int userId);
        Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId);
        Task AddAsync(int userId, int productId);
        Task RemoveAsync(int userId, int productId);
        Task ClearAsync(int userId);
    }
}
