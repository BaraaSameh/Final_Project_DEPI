using DepiFinalProject.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Interfaces
{
    public interface IWishlistRepository
    {
        Task<List<WishlistItemDto>> GetAllAsync(int userId);
        Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId);
        Task<bool> AddAsync(int userId, WishlistItemDto item);
        Task<bool> RemoveAsync(int userId, int productId);
        Task<bool>  ClearAsync(int userId);
    }
}
