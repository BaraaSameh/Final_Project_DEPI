using DepiFinalProject.DTOs;

namespace DepiFinalProject.Services
{
    public class WishlistService
    {
        private readonly WishlistService _repo;

        public WishlistService(WishlistService repo)
        {
            _repo = repo;
        }

        public List<WishlistItemDto> GetAll() => _repo.GetAll();

        public void Add(WishlistItemDto item) => _repo.Add(item);

        public void Remove(int productId) => _repo.Remove(productId);
    }
}
