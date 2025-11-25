using DepiFinalProject.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Core.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartItemDto>> GetAll(int userId);

        Task<CartItemDto> GetByProductId(int userId, int productId);

        Task Add(int userId, CartItemDto item);

        Task UpdateQuantity(int userId, int productId, int quantity);

        Task Remove(int userId, int productId);
        Task Clear(int userId);

    }
}
