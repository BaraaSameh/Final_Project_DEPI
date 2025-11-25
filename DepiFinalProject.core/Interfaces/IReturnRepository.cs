using DepiFinalProject.Core.Models;
using static DepiFinalProject.Core.DTOs.ReturnDto;


namespace DepiFinalProject.Core.Interfaces
{
    public interface IReturnRepository
    {
        Task<IEnumerable<Return>> GetAllAsync();
        Task<Return?> GetByIdAsync(int id);
        Task<Return> CreateAsync(Return returnRequest);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteReturnAsync(int id);
        Task<bool> CancelAsync(int id);
        Task<Return?> GetByOrderItemIdAsync(int orderItemId);
        Task<IEnumerable<Return?>> GetByUserIdAsync(int userId);
 
    }
}
