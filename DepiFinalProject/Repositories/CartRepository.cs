using System.Collections.Concurrent;
using DepiFinalProject.Data;
using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;
        public CartRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<CartItemDto>> GetAll(int userId)
        {
            return await _context.Carts
                .Where(c => c.UserID == userId)
                .Include(c => c.Product)
                .Select(c => new CartItemDto
                {
                    ProductId = c.ProductID,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity
                })
                .ToListAsync();
        }

        public async Task<CartItemDto?> GetByProductId(int userId, int productId)
        {
            return await _context.Carts
                .Where(c => c.UserID == userId && c.ProductID == productId)
                .Include(c => c.Product)
                .Select(c => new CartItemDto
                {
                    ProductId = c.ProductID,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity
                })
                .FirstOrDefaultAsync();
        }

        public async Task Add(int userId, CartItemDto item)
        {
            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserID == userId && c.ProductID == item.ProductId);

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                _context.Carts.Update(existing);
            }
            else
            {
                var cart = new Cart
                {
                    UserID = userId,
                    ProductID = item.ProductId,
                    Quantity = item.Quantity,
                    AddAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
            }

            await _context.SaveChangesAsync();
        }
        public async Task Clear(int userId)
        {
            var userItems = _context.Carts.Where(c => c.UserID == userId);
            _context.Carts.RemoveRange(userItems);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantity(int userId, int productId, int quantity)
        {
            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserID == userId && c.ProductID == productId);

            if (existing != null)
            {
                if (quantity <= 0)
                    _context.Carts.Remove(existing);
                else
                    existing.Quantity = quantity;

                await _context.SaveChangesAsync();
            }
        }

        public async Task Remove(int userId, int productId)
        {
            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserID == userId && c.ProductID == productId);

            if (existing != null)
            {
                _context.Carts.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
