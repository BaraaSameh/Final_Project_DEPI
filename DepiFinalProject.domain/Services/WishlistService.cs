using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
namespace DepiFinalProject.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartService _cartService;

        public WishlistService(IWishlistRepository wishlistRepository, IProductRepository productRepository, ICartService cartService)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _cartService = cartService;
        }

        public async Task<List<WishlistItemDto>> GetAllAsync(int userId)
        {
            var wishlist = await _wishlistRepository.GetAllAsync(userId);
            return wishlist ?? new List<WishlistItemDto>();
        }
        public async Task<bool> MoveAllToCartAsync(int userId)
        {
            var wishlist = await _wishlistRepository.GetAllAsync(userId);

            if (wishlist == null || !wishlist.Any())
                return false;

            foreach (var item in wishlist)
            {
                await _cartService.AddAsync(userId, new AddToCartRequestDto
                {
                    ProductId = item.ProductId,
                    Quantity = 1
                });

                await _wishlistRepository.RemoveAsync(userId, item.ProductId);
            }

            return true;
        }

        public async Task<bool> MoveItemToCartAsync(int userId, int productId)
        {
            var item = await _wishlistRepository.GetByProductIdAsync(userId, productId);

            if (item == null)
                return false;

            await _cartService.AddAsync(userId, new AddToCartRequestDto
            {
                ProductId = productId,
                Quantity = 1
            });

            await _wishlistRepository.RemoveAsync(userId, productId);

            return true;
        }

        public async Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId)
        {
            return await _wishlistRepository.GetByProductIdAsync(userId, productId);
        }

        public async Task<bool> AddAsync(int userId, int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            var existingItem = await _wishlistRepository.GetByProductIdAsync(userId, productId);
            if (existingItem != null)
                throw new InvalidOperationException($"Product with ID {productId} is already in your wishlist.");

            var item = new WishlistItemDto
            {
                ProductId = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price
            };

            await _wishlistRepository.AddAsync(userId, item);
            return true; 
        }

        public async Task<bool> RemoveAsync(int userId, int productId)
        {
            var existingItem = await _wishlistRepository.GetByProductIdAsync(userId, productId);
            if (existingItem == null)
                throw new KeyNotFoundException($"Product with ID {productId} is not in the wishlist.");

            await _wishlistRepository.RemoveAsync(userId, productId);
            return true; 
        }

        public async Task<bool> ClearAsync(int userId)
        {
            var wishlist = await _wishlistRepository.GetAllAsync(userId);
            if (wishlist == null || wishlist.Count == 0)
                throw new InvalidOperationException("Wishlist is already empty.");

            await _wishlistRepository.ClearAsync(userId);
            return true; 
        }
    }
}
