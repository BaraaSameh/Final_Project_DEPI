using DepiFinalProject.Core.Commmon.Pagination;
﻿using DepiFinalProject.InfraStructure.Data;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using static DepiFinalProject.Core.DTOs.ProductDTO;

namespace DepiFinalProject.Infrastructurenamespace.Repositories
{
    public class ProductRepository : IProductRepository
    {
        protected readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddImagesAsync(int productId, List<string> imageUrls,List<string>imagepublicid)
        {
            var images = imageUrls.Select((url, index) => new ProductImage
            {
                ProductId = productId,
                ImageUrl = url,
                imagepublicid = imagepublicid[index]
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
        public async Task<ProductImage> getimagebyid(int imageid,int productid)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.ImageId == imageid&&i.ProductId==productid);
            if (image != null) { return image; }
            else
            {
                return null; 
            }
        }
        public async Task<List<ProductImage>> GetProductImagesAsync(int productId) {
            var images = _context.ProductImages.Where(i => i.ProductId == productId).ToList();

            if (!images.Any()) return null;
            return images;
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
                .Include(p => p.Images)
                .Include(p => p.FlashSaleProducts)
                   .ThenInclude(fsp => fsp.FlashSale)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int CategoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .Include(p => p.Images)
                .Where(p => p.CategoryID == CategoryId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetByUserIdAsync(int userId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .Include(p => p.Images)
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
                .Include(p => p.Images)
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
        public async Task<PagedResult<Product>> GetProductsAsync(ProductFilterParameters parameters)
        {
            // Start with base query including navigation properties
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.user)
                .Include(p => p.Images)
                .AsNoTracking()
                .AsQueryable();

            // Apply Category filter if provided
            if (parameters.CategoryID.HasValue)
            {
                query = query.Where(p => p.CategoryID == parameters.CategoryID.Value);
            }

            // Default sorting: newest products first
            query = query.OrderByDescending(p => p.CreatedAt);

            // Apply pagination using extension method
            return await query.ToPaginatedListAsync(
                parameters.PageNumber,
                parameters.PageSize
            );
        }
        public async Task<bool> AddProductToFlashSaleAsync(int productId, FlashSaleProduct flashSaleProduct)
        {
            //check if product exist
            var productExists = await _context.Products.AnyAsync(p => p.ProductID == productId);
            if (!productExists)
                return false;

            //check if flash sale exist
            var flashSaleExists = await _context.FlashSales.AnyAsync(fs => fs.FlashSaleID == flashSaleProduct.FlashSaleID);
            if (!flashSaleExists)
                return false;

            
            var alreadyExists = await _context.FlashSaleProducts
                .AnyAsync(fsp => fsp.ProductID == productId && fsp.FlashSaleID == flashSaleProduct.FlashSaleID);

            if (alreadyExists)
                return false;

            // add to FlashSaleProducts
            flashSaleProduct.ProductID = productId;
            await _context.FlashSaleProducts.AddAsync(flashSaleProduct);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveProductFromFlashSaleAsync(int productId, int flashSaleId)
        {
            var flashSaleProduct = await _context.FlashSaleProducts
                .FirstOrDefaultAsync(fsp => fsp.ProductID == productId && fsp.FlashSaleID == flashSaleId);

            if (flashSaleProduct == null)
                return false;

            _context.FlashSaleProducts.Remove(flashSaleProduct);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Product?> GetProductWithFlashSaleAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.FlashSaleProducts)
                    .ThenInclude(fsp => fsp.FlashSale)
                .FirstOrDefaultAsync(p => p.ProductID == productId);
        }
    }
}