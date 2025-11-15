using DepiFinalProject.DTOs;

namespace DepiFinalProject.Interfaces
{
    public interface ICartRepository
    {
        List<CartItemDto> GetAll();
        CartItemDto GetByProductId(int productId);
        void Add(CartItemDto item);
        void UpdateQuantity(int productId, int quantity);
        void Remove(int productId);
    }
}
