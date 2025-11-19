using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DepiFinalProject.DTOs.ProductDTO;

namespace DepiFinalProject.Services
{
    public class ProductService : IProductService
    {
        protected readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(IProductRepository productRepository, IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Invalid User.");

            return userId;
        }
        public async Task<bool> AddImagesAsync(int productId, List<string> imageUrls)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new Exception("Product not found.");

            int userId = GetCurrentUserId();

            if (product.userid != userId)
                throw new UnauthorizedAccessException("You can only upload images to your own products.");

            await _productRepository.AddImagesAsync(productId, imageUrls);
            return true;
        }
        public async Task<bool> DeleteImageAsync(int productId, int imageId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new Exception("Product not found.");

            int userId = GetCurrentUserId();

            if (product.userid != userId)
                throw new UnauthorizedAccessException("You cannot delete images from another seller's product.");

            return await _productRepository.DeleteImageAsync(imageId, productId);
        }
        public async Task<bool> DeleteAllImagesAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new Exception("Product not found.");

            int userId = GetCurrentUserId();

            if (product.userid != userId)
                throw new UnauthorizedAccessException("You cannot delete images from another seller's product.");

            return await _productRepository.DeleteAllImagesAsync(productId);
        }

        public async Task<ProductResponseDTO?> CreateAsync([FromBody] CreateProductDTO dto)
        {
            // Get the current user's ID from the claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");

            var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId);
            if (!categoryExists)
                throw new Exception($"Invalid Category ID: {dto.CategoryId}. The category does not exist.");

            var userExists = await _productRepository.UserExistsAsync(userId);
            if (!userExists)
                throw new Exception($"Invalid User ID: {userId}. The user does not exist.");

            var product = new Product
            {
                CategoryID = dto.CategoryId,
                userid = userId,
                ProductName = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageURL = dto.ImageUrl ?? "/images/default-product.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productRepository.CreateNewAsync(product);
            return MapToResponseDto(createdProduct);
        }
        public async Task<IEnumerable<ProductResponseDTO>> GetByUserIdAsync(int userId)
        {
            var products = await _productRepository.GetByUserIdAsync(userId);
            return products.Select(MapToResponseDto);
        }
        public async Task<bool> DeleteAsync(int productId)
        {
            return await _productRepository.DeleteAsync(productId);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return products.Select(MapToResponseDto);
        }

        public async Task<ProductDetailsDTO?> GetById(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            return MapToDetailDto(product);
        }

        public async Task<ProductResponseDTO?> UpdateAsync(int productId, [FromBody] UpdateProductDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Cannot update. Product with ID {productId} not found.");

            // Optional: Verify that the current user is the owner or an admin
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                if (product.userid != currentUserId && userRole != "admin")
                    throw new UnauthorizedAccessException("You are not authorized to update this product.");
            }

            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
                var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId.Value);
                if (!categoryExists)
                    throw new Exception($"Invalid Category ID: {dto.CategoryId.Value}. The category does not exist.");

                product.CategoryID = dto.CategoryId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                product.ProductName = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                product.Description = dto.Description;

            if (dto.Price.HasValue && dto.Price.Value > 0)
                product.Price = dto.Price.Value;

            if (dto.Stock.HasValue && dto.Stock.Value >= 0)
                product.Stock = dto.Stock.Value;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                product.ImageURL = dto.ImageUrl;

            var updatedProduct = await _productRepository.UpdateAsync(product);
            if (updatedProduct == null)
                throw new Exception("Product update failed. Please try again.");

            return MapToResponseDto(updatedProduct);
        }

        private ProductResponseDTO MapToResponseDto(Product product)
        {
            return new ProductResponseDTO
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
                UserId = product.userid,
                SellerName = product.user?.UserFirstName +" "+ product.user?.UserLastName ?? "Unknown",
                SellerEmail = product.user?.UserEmail ?? "Unknown",
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageURL = product.ImageURL,
                CreatedAt = product.CreatedAt
            };
        }

        private ProductDetailsDTO MapToDetailDto(Product product)
        {
            var reviews = product.Reviews?.ToList() ?? new List<Review>();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return new ProductDetailsDTO
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
                UserId = product.userid,
                SellerName = product.user?.UserFirstName + " " + product.user?.UserLastName ?? "Unknown",
                SellerEmail = product.user?.UserEmail ?? "Unknown",
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageURL = product.ImageURL,
                CreatedAt = product.CreatedAt,
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = reviews.Count
            };
        }
    }
}