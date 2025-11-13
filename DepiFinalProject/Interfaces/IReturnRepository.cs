using DepiFinalProject.Models;


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

    }
}
