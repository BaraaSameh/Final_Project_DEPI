using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;

namespace DepiFinalProject.Services
{
    public class CategoryService:ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(c => new CategoryDTO
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IconUrl = c.IconUrl,
                ProductCount = c.Products?.Count ?? 0
            });
        }

      
        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return null;

            return new CategoryDTO
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ProductCount = category.Products?.Count ?? 0
            };
        }

  
        public async Task<CategoryDTO> CreateCategoryAsync(CategoryInputDTO inputDto)
        {
            // تحويل DTO → Entity
            var category = new Category
            {
                CategoryName = inputDto.CategoryName,
                Description = inputDto.Description,
                IconUrl = inputDto.IconUrl
            };

           
            var createdCategory = await _categoryRepository.AddAsync(category);

            // تحويل Entity → DTO
            return new CategoryDTO
            {
                CategoryID = createdCategory.CategoryID,
                CategoryName = createdCategory.CategoryName,
                Description = createdCategory.Description,
                ProductCount = 0
            };
        }

 
        public async Task<bool> UpdateCategoryAsync(int id, CategoryInputDTO inputDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return false;

            category.CategoryName = inputDto.CategoryName;
            category.Description = inputDto.Description;
            category.IconUrl = inputDto.IconUrl;

            await _categoryRepository.UpdateAsync(category);
            return true;
        }


        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null)
            {
                // القسم غير موجود أصلًا، لا يوجد ما يمكن حذفه
                return false;
            }
            if (category.Products != null && category.Products.Any())
            {
                // لا يمكن الحذف لأن هناك منتجات مرتبطة.
                return false;
            }

            await _categoryRepository.DeleteAsync(category);
            return true;
        }

    }
}
