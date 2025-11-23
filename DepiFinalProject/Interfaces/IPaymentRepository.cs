using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(string id);
        Task<List<Payment>> GetByUserIdAsync(int userId);
        Task<Payment> UpdateAsync(Payment payment);
        Task<List<Payment>> GetAllAsync();
        Task<Payment?> GetByPayPalOrderIdAsync(string paypalOrderId);
    }
}
