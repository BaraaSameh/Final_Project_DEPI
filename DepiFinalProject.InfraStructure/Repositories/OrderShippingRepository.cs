using DepiFinalProject.InfraStructure.Data;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Infrastructurenamespace.Repositories
{
    public class OrderShippingRepository: IOrderShippingRepository
    {

        private readonly AppDbContext _context;

        public OrderShippingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderShipping> CreateAsync(OrderShipping orderShipping)
        {
            await _context.OrderShippings.AddAsync(orderShipping);
            await _context.SaveChangesAsync();
            return orderShipping;
        }

        public async Task<bool> RemoveByShippingIdAsync(int shippingId)
        {
            var orderShippings = await _context.OrderShippings
                .Where(os => os.ShippingID == shippingId)
                .ToListAsync();

            if (!orderShippings.Any())
                return false;

            _context.OrderShippings.RemoveRange(orderShippings);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveByOrderIdAsync(int orderId)
        {
            var orderShippings = await _context.OrderShippings
                .Where(os => os.OrderID == orderId)
                .ToListAsync();

            if (!orderShippings.Any())
                return false;

            _context.OrderShippings.RemoveRange(orderShippings);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ICollection<OrderShipping>> GetByShippingIdAsync(int shippingId)
        {
            return await _context.OrderShippings
                .Include(os => os.Order)
                .Include(os => os.Shipping)
                .Where(os => os.ShippingID == shippingId)
                .ToListAsync();
        }

        public async Task<ICollection<OrderShipping>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderShippings
                .Include(os => os.Order)
                .Include(os => os.Shipping)
                .Where(os => os.OrderID == orderId)
                .ToListAsync();
        }
    }
}
