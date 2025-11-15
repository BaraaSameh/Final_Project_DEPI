using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IOrderShippingRepository
    {
        Task<OrderShipping> CreateAsync(OrderShipping orderShipping);
        Task<bool> RemoveByShippingIdAsync(int shippingId);
        Task<bool> RemoveByOrderIdAsync(int orderId);
        Task<ICollection<OrderShipping>> GetByShippingIdAsync(int shippingId);
        Task<ICollection<OrderShipping>> GetByOrderIdAsync(int orderId);
    }
}
