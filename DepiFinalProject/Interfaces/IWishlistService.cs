using DepiFinalProject.DTOs;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Threading.Tasks;
=======
>>>>>>> 2efc83d (initial user commit)

namespace DepiFinalProject.Interfaces
{
    public interface IWishlistService
    {
<<<<<<< HEAD
        Task<List<WishlistItemDto>> GetAllAsync(int userId);
        Task<WishlistItemDto?> GetByProductIdAsync(int userId, int productId);
        Task AddAsync(int userId, int productId);
        Task RemoveAsync(int userId, int productId);
        Task ClearAsync(int userId);
=======
        List<WishlistItemDto> GetAll();
        WishlistItemDto GetByProductId(int productId);
        void Add(WishlistItemDto item);
        void Remove(int productId);
>>>>>>> 2efc83d (initial user commit)
    }
}
