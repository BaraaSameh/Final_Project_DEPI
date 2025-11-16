using DepiFinalProject.DTOs.Reviews;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(int productId);
        Task<ReviewResponseDto> AddReviewAsync(int userId, ReviewCreateDto dto);
        Task<ReviewResponseDto> UpdateReviewAsync(int id, int userId, ReviewUpdateDto dto, bool isAdmin);
        Task<bool> DeleteReviewAsync(int id, int userId, bool isAdmin);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByUserIdAsync(int userId);
    }
}
