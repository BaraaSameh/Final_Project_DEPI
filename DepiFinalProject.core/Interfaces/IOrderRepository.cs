
using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetByUserAsync(int userId);
        Task<Order> CreateAsync(Order order);
        Task<Order?> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int orderId);
        Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);
        Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);
        Task<bool> RemoveOrderItemAsync(int orderItemId);
        Task<bool> HasuserorderedproductAsync(int userId, int productId);
        Task<Order?> GetByPaymentid(int paymentid);
    }
}
