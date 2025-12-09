using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IShippingService
    {
        Task<ICollection<Shipping>> GetAllShippingsAsync();
        Task<Shipping?> GetShippingByIdAsync(int shippingId);
        Task<Shipping> CreateShippingAsync(Shipping newShipping);
        Task<Shipping?> UpdateShippingAsync(Shipping updatedShipping);
        Task UpdateShippingStatus(int shippingId, string newStatus);
        Task CalcEstimatedDelivery(int shippingId, DateTime newEstimatedDate);
    }
}
