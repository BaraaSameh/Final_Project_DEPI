using DepiFinalProject.InfraStructure.Data;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Infrastructurenamespace.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserID == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Address> GetAddressByIdAsync(int addressId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AddressID == addressId);
        }

        public async Task<Address> AddAddressAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAddressAsync(Address address)
        {
            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddressExistsAsync(int addressId)
        {
            return await _context.Addresses.AnyAsync(a => a.AddressID == addressId);
        }
    }
}
