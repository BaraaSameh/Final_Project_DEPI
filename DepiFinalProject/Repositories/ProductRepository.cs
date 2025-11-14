
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

        public async Task<Product> CreateNewAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
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
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int CategoryId)
        {
            return await _context.Products.Include(p => p.Category)
              .Where(p => p.CategoryID == CategoryId).ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(int ProductId)
        {
            return await _context.Products.Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductID == ProductId);
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryID == categoryId);
        }
    }
}
