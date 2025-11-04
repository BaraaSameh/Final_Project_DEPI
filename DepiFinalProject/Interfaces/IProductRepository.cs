using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int ProductId);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<Product> CreateNewAsync(Product product);
        Task<Product?> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int productId);



    }
}
