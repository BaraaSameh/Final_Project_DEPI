using DepiFinalProject.Core.DTOs;

namespace DepiFinalProject.Services
{
    public interface IFlashSaleService
    {
        Task<IEnumerable<FlashSaleDto>> GetAllAsync();
        Task<FlashSaleDto?> GetByIdAsync(int id);
        Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto);
        Task<FlashSaleDto?> UpdateAsync(int id, UpdateFlashSaleDto dto);
        Task<bool> DeleteAsync(int id);
    }
}