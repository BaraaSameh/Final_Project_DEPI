using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;

namespace DepiFinalProject.Repositories
{
    public class CartRepository: ICartRepository
    {
        private readonly List<CartItemDto> _cart = new();

        public List<CartItemDto> GetAll() => _cart;

        public CartItemDto GetByProductId(int productId) =>
            _cart.FirstOrDefault(i => i.ProductId == productId);

        public void Add(CartItemDto item)
        {
            var existing = GetByProductId(item.ProductId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                _cart.Add(item);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = GetByProductId(productId);
            if (item != null)
                item.Quantity = quantity;
        }

        public void Remove(int productId) =>
            _cart.RemoveAll(i => i.ProductId == productId);
    }
}
