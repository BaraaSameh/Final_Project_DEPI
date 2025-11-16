using DepiFinalProject.Data;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;
using static DepiFinalProject.DTOs.ReturnDto;

namespace DepiFinalProject.Repositories
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly AppDbContext _context;

        public ReturnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Return>> GetAllAsync()
        {
            return await _context.Returns
                .Include(r => r.OrderItem)
                .ToListAsync();
        }
        public async Task<IEnumerable<Return?>> GetByUserIdAsync(int userId)
        {
            return await _context.Returns
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
        public async Task<Return?> GetByIdAsync(int id)
        {
            return await _context.Returns
                .Include(r => r.OrderItem)
                .FirstOrDefaultAsync(r => r.ReturnID == id);
        }

        public async Task<Return> CreateAsync(Return returnRequest)
        {
            _context.Returns.Add(returnRequest);
            await _context.SaveChangesAsync();
            return returnRequest;
        }
        public async Task<Return?> GetByOrderItemIdAsync(int orderItemId)
        {
            return await _context.Returns
                .Include(r => r.OrderItem)
                .FirstOrDefaultAsync(r => r.OrderItemID == orderItemId && !r.IsCancelled);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var returnRequest = await _context.Returns.FindAsync(id);
            if (returnRequest == null)
                return false;

            returnRequest.Status = status;
            _context.Returns.Update(returnRequest);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteReturnAsync(int id)
        {
            var returnRequest = await _context.Returns.FindAsync(id);
            if (returnRequest == null) return false;
            _context.Returns.Remove(returnRequest);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CancelAsync(int id)
        {
            var returnRequest = await _context.Returns.FindAsync(id);
            if (returnRequest == null)
                return false;

            

            returnRequest.IsCancelled = true;
            returnRequest.Status = "cancelled";
            _context.Returns.Update(returnRequest);
            await _context.SaveChangesAsync();
            return true;
        }
 
    }
}
