using DepiFinalProject.Core.Commmon.Pagination;
﻿using Microsoft.AspNetCore.Http;
using static DepiFinalProject.Core.DTOs.ProductDTO;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDTO>> GetAllAsync();
        Task<ProductDetailsDTO?> GetById(int productId);
        Task<IEnumerable<ProductResponseDTO>> GetByCategoryAsync(int categoryId);
        Task<ProductResponseDTO?> CreateAsync(CreateProductDTO dto);
        Task<ProductResponseDTO?> UpdateAsync(int productId, UpdateProductDTO dto);
        Task<bool> DeleteAsync(int productId);
        Task<IEnumerable<ProductResponseDTO>> GetByUserIdAsync(int userId);

        Task<bool> DeleteAllImagesAsync(int productId);
        Task<bool> DeleteImageAsync(int productId, int imageId);
        Task<bool> AddImagesAsync(int productId, List<IFormFile> images);
        Task<PagedResult<ProductResponseDTO>> GetProductsAsync(ProductFilterParameters parameters);

    }
}
