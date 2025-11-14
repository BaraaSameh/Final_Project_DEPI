using DepiFinalProject.DTOs;

namespace DepiFinalProject.Interfaces
{
    public interface IWishlistService
    {
        List<WishlistItemDto> GetAll();
        WishlistItemDto GetByProductId(int productId);
        void Add(WishlistItemDto item);
        void Remove(int productId);
    }
}
