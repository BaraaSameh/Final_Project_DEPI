using DepiFinalProject.Models;

public interface IProductRepository
{
    Task<Product> CreateNewAsync(Product product);
    Task<bool> DeleteAsync(int ProductId);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByCategoryAsync(int CategoryId);
    Task<Product?> GetByIdAsync(int ProductId);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<bool> UserExistsAsync(int userId);

    // NEW METHOD
    Task<IEnumerable<Product>> GetByUserIdAsync(int userId);
}