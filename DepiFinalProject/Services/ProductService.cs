using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;
using static DepiFinalProject.DTOs.ProductDTO;

namespace DepiFinalProject.Services
{
    public class ProductService : IProductService
    {
        protected readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<ResponseDTO?> CreateAsync(CreateDTO dto)
        {
            var product = new Product
            {
                CategoryID = dto.CategoryId,
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

        public async Task<bool> DeleteAsync(int productId)
        {
            return await _productRepository.DeleteAsync(productId);
        }

        public async Task<IEnumerable<ResponseDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<ResponseDTO>> GetByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return products.Select(MapToResponseDto);
        }

        public async Task<DetailsDTO?> GetById(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return null;
            return MapToDetailDto(product);
        }

        public async Task<ResponseDTO?> UpdateAsync(int productId, UpdateDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return null;
            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
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
            return updatedProduct == null ? null : MapToResponseDto(updatedProduct);

        }
        private ResponseDTO MapToResponseDto(Product product)
        {
            return new ResponseDTO
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageURL = product.ImageURL,
                CreatedAt = product.CreatedAt
            };
        }
        private DetailsDTO MapToDetailDto(Product product)
        {
            var reviews = product.Reviews?.ToList() ?? new List<Review>();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            return new DetailsDTO
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
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
