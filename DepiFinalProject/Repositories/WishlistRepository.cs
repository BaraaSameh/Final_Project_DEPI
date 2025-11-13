using DepiFinalProject.Data;
using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DepiFinalProject.Repositories
{
    public class WishlistRepository: IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WishlistItemDto>> GetAllAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserID == userId)
                .Include(w => w.Product)
                .Select(w => new WishlistItemDto
                {
                    ProductId = w.ProductID,
                    ProductName = w.Product.ProductName,
                    Price = w.Product.Price
                })
                .ToListAsync();
        }

        public async Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId)
        {
            return await _context.Wishlists
                .Where(w => w.UserID == userId && w.ProductID == productId)
                .Include(w => w.Product)
                .Select(w => new WishlistItemDto
                {
                    ProductId = w.ProductID,
                    ProductName = w.Product.ProductName,
                    Price = w.Product.Price
                })
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(int userId, WishlistItemDto item)
        {
            bool exists = await _context.Wishlists
                .AnyAsync(w => w.UserID == userId && w.ProductID == item.ProductId);

            if (!exists)
            {
                var wishlist = new Wishlist
                {
                    UserID = userId,
                    ProductID = item.ProductId,
                    AddedAt = DateTime.UtcNow
                };

                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(int userId, int productId)
        {
            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserID == userId && w.ProductID == productId);

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(int userId)
        {
            var items = _context.Wishlists.Where(w => w.UserID == userId);
            _context.Wishlists.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
