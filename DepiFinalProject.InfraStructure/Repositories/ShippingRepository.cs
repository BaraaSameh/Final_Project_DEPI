using DepiFinalProject.InfraStructure.Data;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Infrastructure.Repositories
{
    public class ShippingRepository : IShippingRepository
    {
        private readonly AppDbContext _context;

        public ShippingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Shipping>> GetAllShippingsAsync()
        {
            return await _context.Shippings
                .Include(s => s.OrderShippings)
                    .ThenInclude(os => os.Order)
                .ToListAsync();
        }

        public async Task<Shipping?> GetShippingByIdAsync(int shippingId)
        {
            return await _context.Shippings
                .Include(s => s.OrderShippings)
                .ThenInclude(os => os.Order)
                .FirstOrDefaultAsync(s => s.ShippingID == shippingId);
        }

        public async Task<Shipping> CreateShippingAsync(Shipping newShipping)
        {
            await _context.Shippings.AddAsync(newShipping);
            await _context.SaveChangesAsync();
            return newShipping;
        }

        public async Task<Shipping?> UpdateShippingAsync(Shipping updatedShipping)
        {
            var existingShipping = await _context.Shippings
                .Include(s => s.OrderShippings)
                    .ThenInclude(os => os.Order)
                .FirstOrDefaultAsync(s => s.ShippingID == updatedShipping.ShippingID);

            if (existingShipping == null)
                return null;

            existingShipping.ShippingStatus = updatedShipping.ShippingStatus;
            existingShipping.EstimatedDelivery = updatedShipping.EstimatedDelivery;

            await _context.SaveChangesAsync();

            return existingShipping;
        }


        public async Task<OrderShipping> AddOrderShippingAsync(OrderShipping orderShipping)
        {
            if (orderShipping == null)
                throw new ArgumentNullException(nameof(orderShipping));

            await _context.OrderShippings.AddAsync(orderShipping);
            await _context.SaveChangesAsync();

            return orderShipping;
        }

        public async Task<bool> DeleteShippingByIdAsync(int shippingId)
        {
            var shipping = await _context.Shippings.FindAsync(shippingId);

            if (shipping == null)
                return false;

            _context.Shippings.Remove(shipping);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

