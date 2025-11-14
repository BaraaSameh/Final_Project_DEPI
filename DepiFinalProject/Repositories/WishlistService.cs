using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;

namespace DepiFinalProject.Repositories
{
    public class WishlistService : IWishlistService
    {
        private readonly List<WishlistItemDto> _wishlist = new();

        public List<WishlistItemDto> GetAll() => _wishlist;

        public WishlistItemDto GetByProductId(int productId) =>
            _wishlist.FirstOrDefault(i => i.ProductId == productId);

        public void Add(WishlistItemDto item) => _wishlist.Add(item);

        public void Remove(int productId) =>
            _wishlist.RemoveAll(i => i.ProductId == productId);
    }
}
