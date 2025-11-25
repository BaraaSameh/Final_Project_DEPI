using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByIdWithProductsAsync(int id);
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task<bool> ExistsAsync(int id);
        Task<int> GetProductCountAsync(int categoryId);
    }
}
