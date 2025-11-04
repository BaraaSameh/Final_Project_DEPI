using static DepiFinalProject.DTOs.ProductDTO;

namespace DepiFinalProject.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ResponseDTO>> GetAllAsync();
        Task<DetailsDTO?> GetById(int productId);
        Task<IEnumerable<ResponseDTO>> GetByCategoryAsync(int categoryId);
        Task<ResponseDTO?> CreateAsync(CreateDTO dto);
        Task<ResponseDTO?> UpdateAsync(int productId, UpdateDTO dto);
        Task<bool> DeleteAsync(int productId);


    }
}
