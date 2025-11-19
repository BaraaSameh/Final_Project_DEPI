using DepiFinalProject.Data;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Repositories
{
    public class ProductRepository : IProductRepository
    {
        protected readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddImagesAsync(int productId, List<string> imageUrls)
        {
            var images = imageUrls.Select(url => new ProductImage
            {
                ProductId = productId,
                ImageUrl = url
            }).ToList();

            await _context.ProductImages.AddRangeAsync(images);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteImageAsync(int imageId, int productId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.ImageId == imageId && i.ProductId == productId);

            if (image == null) return false;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAllImagesAsync(int productId)
        {
            var images = _context.ProductImages.Where(i => i.ProductId == productId).ToList();

            if (!images.Any()) return false;

            _context.ProductImages.RemoveRange(images);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Product> CreateNewAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Load related entities
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            await _context.Entry(product).Reference(p => p.user).LoadAsync();

            return product;
        }

        public async Task<bool> DeleteAsync(int ProductId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == ProductId);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int CategoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .Where(p => p.CategoryID == CategoryId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetByUserIdAsync(int userId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .Where(p => p.userid == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(int ProductId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductID == ProductId);
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            // Reload navigation properties
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            await _context.Entry(product).Reference(p => p.user).LoadAsync();

            return product;
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryID == categoryId);
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.UserID == userId);
        }
    }
}