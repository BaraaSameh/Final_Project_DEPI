using DepiFinalProject.Models;
using static DepiFinalProject.DTOs.ReturnDto;


namespace DepiFinalProject.Interfaces
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
