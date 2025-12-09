using DepiFinalProject.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetAllAsync(int userId);
        Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId);
        Task<bool> AddAsync(int userId, int productId);
        Task<bool> RemoveAsync(int userId, int productId);
        Task<bool> ClearAsync(int userId);
        Task<bool> MoveAllToCartAsync(int userId);
        Task<bool> MoveItemToCartAsync(int userId, int productId);

    }
}
