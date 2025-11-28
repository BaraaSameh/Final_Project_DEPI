using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;

namespace DepiFinalProject.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _productRepository = productRepository;

            _cartRepository = cartRepository;
        }
      
        public async Task<List<CartItemDto>> GetAllAsync(int userId)
        {
            return await _cartRepository.GetAll(userId);
        }

        public async Task<CartItemDto> GetByProductIdAsync(int userId, int productId)
        {
            return await _cartRepository.GetByProductId(userId, productId);
        }

        public async Task AddAsync(int userId, AddToCartRequestDto request)
        {
            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be at least 1.");


            if (request.Quantity <= 0)
                request.Quantity = 1;

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {request.ProductId} not found.");

            var item = new CartItemDto
            {
                ProductId = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = request.Quantity
            };

            await _cartRepository.Add(userId, item);
        }


        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be at least 1.");

            if (quantity < 0)
                quantity = 1; 

            await _cartRepository.UpdateQuantity(userId, productId, quantity);
        }
        public async Task ClearAsync(int userId)
        {
            await _cartRepository.Clear(userId);
        }

        public async Task RemoveAsync(int userId, int productId)
        {
            await _cartRepository.Remove(userId, productId);
        }
    }
}
