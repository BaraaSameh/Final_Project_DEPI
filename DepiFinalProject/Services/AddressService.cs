using DepiFinalProject.Data;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly AppDbContext _context;

        public AddressService(IAddressRepository addressRepository, AppDbContext context)
        {
            _addressRepository = addressRepository;
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetUserAddressesAsync(int userId)
        {
            // التحقق من وجود اليوزر باستخدام DbContext مباشرة
            await CheckUserExistsAsync(userId);

            return await _addressRepository.GetAddressesByUserIdAsync(userId);
        }

        public async Task<Address> GetAddressByIdAsync(int addressId)
        {
            return await _addressRepository.GetAddressByIdAsync(addressId);
        }

        public async Task<Address> CreateAddressAsync(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            if (string.IsNullOrWhiteSpace(address.FullAddress))
                throw new ArgumentException("Full address is required", nameof(address));

            if (string.IsNullOrWhiteSpace(address.City))
                throw new ArgumentException("City is required", nameof(address));

            if (string.IsNullOrWhiteSpace(address.Country))
                throw new ArgumentException("Country is required", nameof(address));

            // التحقق من وجود اليوزر قبل إنشاء العنوان
            await CheckUserExistsAsync(address.UserID);

            return await _addressRepository.AddAddressAsync(address);
        }

        public async Task<Address> UpdateAddressAsync(int addressId, Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var existingAddress = await _addressRepository.GetAddressByIdAsync(addressId);
            if (existingAddress == null)
                throw new KeyNotFoundException($"Address with ID {addressId} not found");

            // التحقق من وجود اليوزر الجديد قبل التحديث
            await CheckUserExistsAsync(address.UserID);

            existingAddress.UserID = address.UserID;
            existingAddress.FullAddress = address.FullAddress;
            existingAddress.City = address.City;
            existingAddress.Country = address.Country;

            return await _addressRepository.UpdateAddressAsync(existingAddress);
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var exists = await _addressRepository.AddressExistsAsync(addressId);
            if (!exists)
                throw new KeyNotFoundException($"Address with ID {addressId} not found");
            return await _addressRepository.DeleteAddressAsync(addressId);
        }
        private async Task CheckUserExistsAsync(int userId)
        {
            // التحقق من وجود اليوزر باستخدام DbContext مباشرة
            var userExists = await _context.Users.AnyAsync(u => u.UserID == userId);
            if (!userExists)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }
        }

    }
}
