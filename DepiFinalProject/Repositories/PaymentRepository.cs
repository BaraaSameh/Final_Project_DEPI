using DepiFinalProject.Data;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DepiFinalProject.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> GetByIdAsync(string id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayPalOrderId == id);
        }

        public async Task<List<Payment>> GetByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.User)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        async Task<Payment?> IPaymentRepository.GetByPayPalOrderIdAsync(string paypalOrderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayPalOrderId == paypalOrderId);
        }
    }
}
