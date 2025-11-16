using static DepiFinalProject.DTOs.ProductDTO;

namespace DepiFinalProject.Interfaces
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



    }
}
