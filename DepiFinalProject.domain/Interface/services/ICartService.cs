using DepiFinalProject.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Core.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemDto>> GetAllAsync(int userId);
        Task<CartItemDto> GetByProductIdAsync(int userId, int productId);
        Task AddAsync(int userId, AddToCartRequestDto request);
        Task UpdateQuantityAsync(int userId, int productId, int quantity);
        Task RemoveAsync(int userId, int productId);
        Task ClearAsync(int userId);
    }
}
