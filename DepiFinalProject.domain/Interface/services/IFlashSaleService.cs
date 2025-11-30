using DepiFinalProject.Core.DTOs;
using DepiFinalProject.core.DTOs;
namespace DepiFinalProject.Services
{
    public interface IFlashSaleService
    {
        Task<IEnumerable<AddProductToFlashSaleDto>> GetAllAsync();
        Task<AddProductToFlashSaleDto?> GetByIdAsync(int id);
        Task<AddProductToFlashSaleDto> CreateAsync(CreateFlashSaleDto dto);
        Task<AddProductToFlashSaleDto?> UpdateAsync(int id, UpdateFlashSaleDto dto);
        Task<bool> DeleteAsync(int id);
    }
}