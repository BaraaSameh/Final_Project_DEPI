using DepiFinalProject.DTOs;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Threading.Tasks;
=======
>>>>>>> 2efc83d (initial user commit)

namespace DepiFinalProject.Interfaces
{
    public interface ICartRepository
    {
<<<<<<< HEAD
        Task<List<CartItemDto>> GetAll(int userId);

        Task<CartItemDto> GetByProductId(int userId, int productId);

        Task Add(int userId, CartItemDto item);

        Task UpdateQuantity(int userId, int productId, int quantity);

        Task Remove(int userId, int productId);
        Task Clear(int userId);

=======
        List<CartItemDto> GetAll();
        CartItemDto GetByProductId(int productId);
        void Add(CartItemDto item);
        void UpdateQuantity(int productId, int quantity);
        void Remove(int productId);
>>>>>>> 2efc83d (initial user commit)
    }
}
