using DepiFinalProject.Models;

namespace DepiFinalProject.Interfaces
{
    public interface IReturnService
    {
        Task<IEnumerable<Return>> GetAllReturnsAsync();
        Task<Return?> GetReturnByIdAsync(int id);
        Task<Return?> GetReturnsByOrderItemIdAsync(int orderItemId);
        Task<Return> RequestReturnAsync(int orderItemId, string reason);
        Task<bool> UpdateReturnStatusAsync(int id, string status);
        Task<bool> DeleteReturnAsync(int id);
        Task<bool> CancelReturnAsync(int id);

    }
}
