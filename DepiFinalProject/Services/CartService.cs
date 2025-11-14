using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;

namespace DepiFinalProject.Services
{
    public class CartService
    {
        private readonly ICartRepository _repo;

        public CartService(ICartRepository repo)
        {
            _repo = repo;
        }

        public List<CartItemDto> GetAll() => _repo.GetAll();

        public void Add(CartItemDto item) => _repo.Add(item);

        public void UpdateQuantity(int productId, int quantity) =>
            _repo.UpdateQuantity(productId, quantity);

        public void Remove(int productId) => _repo.Remove(productId);
    }
}
