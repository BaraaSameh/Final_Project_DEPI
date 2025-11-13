using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;

namespace DepiFinalProject.Services
{
    public class ReturnService : IReturnService
    {
        private readonly IReturnRepository _returnRepository;

        public ReturnService(IReturnRepository returnRepository)
        {
            _returnRepository = returnRepository;
        }

        public async Task<IEnumerable<Return>> GetAllReturnsAsync()
        {
            return await _returnRepository.GetAllAsync();
        }

        public async Task<Return?> GetReturnByIdAsync(int id)
        {
            return await _returnRepository.GetByIdAsync(id);
        }
        public async Task<Return?> GetReturnsByOrderItemIdAsync(int orderItemId)
        {
            return await _returnRepository.GetByOrderItemIdAsync(orderItemId);
        }

        public async Task<Return> RequestReturnAsync(int orderItemId, string reason)
        {
            var newReturn = new Return
            {
                OrderItemID = orderItemId,
                Reason = reason,
                Status = "In Process",
                RequestedAt = DateTime.UtcNow
            };

            return await _returnRepository.CreateAsync(newReturn);
        }

        public async Task<bool> UpdateReturnStatusAsync(int id, string status)
        {
            return await _returnRepository.UpdateStatusAsync(id, status);
        }
        public async Task<bool> DeleteReturnAsync(int id)
        {
            return await _returnRepository.DeleteReturnAsync(id);
        }
        public async Task<bool> CancelReturnAsync(int id)
        {
            return await _returnRepository.CancelAsync(id);
        }

    }
}
