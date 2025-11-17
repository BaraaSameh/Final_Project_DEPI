using DepiFinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DepiFinalProject.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<Review> AddReviewAsync(Review review);
        Task<Review> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int id);
        Task<Review?> GetByIdAsync(int id);
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);
        Task<IEnumerable<Review>> getreviewsbyuser(int userid);
    }
}
