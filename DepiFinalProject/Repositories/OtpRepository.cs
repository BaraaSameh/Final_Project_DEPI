using DepiFinalProject.Data;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Migrations;
using DepiFinalProject.Models;
using DepiFinalProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _context;
        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Models.OTP entry) => await _context.OtpEntries.AddAsync(entry);

        public Task<Models.OTP> GetByIdAsync(int id) => _context.OtpEntries.FindAsync(id).AsTask();

        public Task<Models.OTP> GetValidOtpAsync(int userId, string purpose, string codeHash = null)
        {
            var q = _context.OtpEntries
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

            if (!string.IsNullOrEmpty(codeHash))
                q = q.Where(o => o.OtpHash == codeHash);

            return q.OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

    }
}
