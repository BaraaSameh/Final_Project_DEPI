using DepiFinalProject.Core.Models;

namespace DepiFinalProject.Core.Interfaces
{
    public interface IShippingService
    {
        Task<ICollection<Shipping>> GetAllShippingsAsync();
        Task<Shipping?> GetShippingByIdAsync(int shippingId);
        Task<Shipping> CreateShippingAsync(Shipping newShipping);
        Task<Shipping?> UpdateShippingAsync(Shipping updatedShipping);
        void UpdateShippingStatus(int shippingId, string newStatus);
        void CalcEstimatedDelivery(int shippingId, DateTime newEstimatedDate);
    }
}
