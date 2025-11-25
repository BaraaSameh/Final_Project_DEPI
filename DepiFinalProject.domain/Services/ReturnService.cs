using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;

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
    public async Task<IEnumerable<Return?>> GetReturnRequestsByUserIdAsync(int userId)
    {
        return await _returnRepository.GetByUserIdAsync(userId);
    }
    public async Task<Return> RequestReturnAsync(int userId, int orderItemId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        // ❗ Prevent duplicate return requests
        var existing = await _returnRepository.GetByOrderItemIdAsync(orderItemId);
        if (existing != null)
            throw new InvalidOperationException("A return request for this order item already exists.");

        var newReturn = new Return
        {
            UserId = userId,
            OrderItemID = orderItemId,
            Reason = reason,
            Status = "In Process",
            RequestedAt = DateTime.UtcNow
        };

        return await _returnRepository.CreateAsync(newReturn);
    }

    public async Task<bool> UpdateReturnStatusAsync(int id, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentNullException(nameof(status));

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
