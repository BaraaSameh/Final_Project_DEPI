using DepiFinalProject.Commmon.Pagination;
using static DepiFinalProject.Core.DTOs.ProductDTO;
using DepiFinalProject.Core.Models;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetProductsAsync(ProductFilterParameters parameters);
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
    Task AddImagesAsync(int productId, List<string> imageUrls, List<string> imagepublicid);
    Task<bool> DeleteImageAsync(int imageId, int productId);
    Task<bool> DeleteAllImagesAsync(int productId);
    Task<ProductImage> getimagebyid(int imageid,int productid);
    Task<List<ProductImage>> GetProductImagesAsync(int productId);

}