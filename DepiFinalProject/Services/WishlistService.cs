using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;

        public WishlistService(IWishlistRepository wishlistRepository, IProductRepository productRepository)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
        }

        public async Task<List<WishlistItemDto>> GetAllAsync(int userId)
        {
            return await _wishlistRepository.GetAllAsync(userId);
        }

        public async Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId)
        {
            return await _wishlistRepository.GetByProductIdAsync(userId, productId);
        }

        public async Task AddAsync(int userId, int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            var item = new WishlistItemDto
            {
                ProductId = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price
            };

            await _wishlistRepository.AddAsync(userId, item);
        }

        public async Task RemoveAsync(int userId, int productId)
        {
            await _wishlistRepository.RemoveAsync(userId, productId);
        }

        public async Task ClearAsync(int userId)
        {
            await _wishlistRepository.ClearAsync(userId);
        }
    }
}
