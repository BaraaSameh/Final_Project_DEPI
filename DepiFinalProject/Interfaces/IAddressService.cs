using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<Address>> GetUserAddressesAsync(int userId);
        Task<Address> GetAddressByIdAsync(int addressId);
        Task<Address> CreateAddressAsync(Address address);
        Task<Address> UpdateAddressAsync(int addressId, Address address);
        Task<bool> DeleteAddressAsync(int addressId);
    }
}
