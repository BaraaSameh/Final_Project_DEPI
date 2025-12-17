using DepiFinalProject.InfraStructure.Data;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Infrastructurenamespace.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        protected readonly AppDbContext _context;
        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.OrderID) ??
                throw new InvalidOperationException("Failed to retrieve created order");
        }

        public async Task<bool> DeleteAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID", nameof(orderId));

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderShippings)
                    .ThenInclude(os => os.Shipping)  
                .ToListAsync();
        }


        public async Task<Order?> GetByIdAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID", nameof(orderId));

            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderShippings)
                    .ThenInclude(os => os.Shipping)  
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
        }


        public async Task<IEnumerable<Order>> GetByUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderShippings)
                    .ThenInclude(os => os.Shipping)
                .Where(o => o.UserID == userId)
                .ToListAsync();
        }
        public async Task<Order?> GetByPaymentid(int paymentid)
        {
            if (paymentid <= 0)
                throw new ArgumentException("Invalid payment ID", nameof(paymentid));

            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                 .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Payments.Any(p => p.PaymentID == paymentid));
        }

        public async Task<Order?> UpdateAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderID <= 0)
                throw new ArgumentException("Invalid order ID", nameof(order));

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
        //orderItem
        public async Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId)
        {
            if (orderItemId <= 0)
                throw new ArgumentException("Invalid order item ID", nameof(orderItemId));

            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.OrderItemID == orderItemId);
        }

        public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();

            // Reload with related data
            return await GetOrderItemByIdAsync(orderItem.OrderItemID)
                ?? throw new InvalidOperationException("Failed to retrieve created order item");
        }

        public async Task<bool> RemoveOrderItemAsync(int orderItemId)
        {
            if (orderItemId <= 0)
                throw new ArgumentException("Invalid order item ID", nameof(orderItemId));

            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.OrderItemID == orderItemId);

            if (orderItem == null)
                return false;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> HasuserorderedproductAsync(int userId, int productId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.Order.UserID == userId && oi.ProductID == productId);
        }
    }
}
