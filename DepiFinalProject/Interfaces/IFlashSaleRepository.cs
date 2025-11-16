using DepiFinalProject.Models;

namespace DepiFinalProject.Repositories
{
    public interface IFlashSaleRepository
    {
        // GET: Get all flash sales
        Task<IEnumerable<FlashSale>> GetAllAsync();

        // GET: Get flash sale by ID
        Task<FlashSale?> GetByIdAsync(int id);

        // POST: Create new flash sale
        Task<FlashSale> CreateAsync(FlashSale flashSale);

        // PUT: Update flash sale
        Task<FlashSale?> UpdateAsync(FlashSale flashSale);

        // DELETE: Delete flash sale
        Task<bool> DeleteAsync(int id);

        // Helper: Check if exists
        Task<bool> ExistsAsync(int id);
    }
}