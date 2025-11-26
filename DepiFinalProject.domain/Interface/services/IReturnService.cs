using DepiFinalProject.Core.Models;
using static DepiFinalProject.Core.DTOs.ReturnDto;
namespace DepiFinalProject.Core.Interfaces
{
    public interface IReturnService
    {
        Task<IEnumerable<Return>> GetAllReturnsAsync();
        Task<IEnumerable<Return?>> GetReturnRequestsByUserIdAsync(int userId);

        Task<Return?> GetReturnByIdAsync(int id);
        Task<Return?> GetReturnsByOrderItemIdAsync(int orderItemId);
        Task<Return> RequestReturnAsync(int userId, int orderItemId, string reason);
        Task<bool> UpdateReturnStatusAsync(int id, string status);
        Task<bool> DeleteReturnAsync(int id);
        Task<bool> CancelReturnAsync(int id);
        Task<Return> ProcessRefundAsync(int returnId); // handel refunds

    }
}
