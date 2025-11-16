using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IShippingRepository
    {
        Task<ICollection<Shipping>> GetAllShippingsAsync();
        Task<Shipping?> GetShippingByIdAsync(int shippingId);
        Task<Shipping> CreateShippingAsync(Shipping newShipping);
        Task<Shipping?> UpdateShippingAsync(Shipping updatedShipping);
        Task<bool> DeleteShippingByIdAsync(int shippingId);

    }
}
