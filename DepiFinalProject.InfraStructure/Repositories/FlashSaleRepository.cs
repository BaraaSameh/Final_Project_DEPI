using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using DepiFinalProject.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Infrastructurenamespace.Repositories
{
    public class FlashSaleRepository : IFlashSaleRepository
    {
        private readonly AppDbContext _context;

        public FlashSaleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FlashSale>> GetAllAsync()
        {
            return await _context.FlashSales
                .Include(fs => fs.FlashSaleProducts)
                .OrderByDescending(fs => fs.CreatedAt)
                .ToListAsync();
        }

        public async Task<FlashSale?> GetByIdAsync(int id)
        {
            return await _context.FlashSales
                .Include(fs => fs.FlashSaleProducts)
                .FirstOrDefaultAsync(fs => fs.FlashSaleID == id);
        }

        public async Task<FlashSale> CreateAsync(FlashSale flashSale)
        {
            flashSale.CreatedAt = DateTime.Now;
            flashSale.IsActive = true;

            await _context.FlashSales.AddAsync(flashSale);
            await _context.SaveChangesAsync();

            return flashSale;
        }

        public async Task<FlashSale?> UpdateAsync(FlashSale flashSale)
        {
            var existing = await _context.FlashSales.FindAsync(flashSale.FlashSaleID);

            if (existing == null)
                return null;

            _context.Entry(existing).CurrentValues.SetValues(flashSale);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var flashSale = await _context.FlashSales.FindAsync(id);

            if (flashSale == null)
                return false;

            _context.FlashSales.Remove(flashSale);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.FlashSales.AnyAsync(fs => fs.FlashSaleID == id);
        }
    }
}