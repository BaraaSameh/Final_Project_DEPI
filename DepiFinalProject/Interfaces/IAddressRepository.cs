using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId);
        Task<Address> GetAddressByIdAsync(int addressId);
        Task<Address> AddAddressAsync(Address address);
        Task<Address> UpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<bool> AddressExistsAsync(int addressId);
    }
}
