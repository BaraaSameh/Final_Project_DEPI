using DepiFinalProject.DTOs;
using DepiFinalProject.Models;
using static DepiFinalProject.DTOs.OrderDto;

namespace DepiFinalProject.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDTO>> GetAllAsync();
        Task<OrderDetailsDTO?> GetById(int orderId);
        Task<IEnumerable<OrderResponseDTO>> GetByUserAsync(int userId);
        Task<OrderResponseDTO?> CreateAsync(CreateOrderDTO dto);
        Task<OrderResponseDTO?> UpdateStatusAsync(int orderId, UpdateOrderStatusDTO dto);
        Task<bool> CancelAsync(int orderId);
        // New methods for OrderItems
        Task<IEnumerable<OrderItemResponseDTO>> GetOrderItemsAsync(int orderId);
        Task<OrderItemResponseDTO?> AddOrderItemAsync(int orderId, AddOrderItemDTO dto);
        Task<Order> GetByIdAsync(int orderId);
    }
}
