using DepiFinalProject.Core.DTOs;
using System.Threading.Tasks;

namespace DepiFinalProject.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> CreateCategoryAsync(CategoryInputDTO inputDto);
        Task<bool> UpdateCategoryAsync(int id, CategoryInputDTO inputDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> UploadCategoryIconAsync(int categoryId, Microsoft.AspNetCore.Http.IFormFile file);
        Task<bool> DeleteCategoryIconAsync(int categoryId);
    }
}
