using DepiFinalProject.DTOs;
using DepiFinalProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly WishlistService _service;

        public WishlistController(WishlistService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<List<WishlistItemDto>> GetWishlist() => Ok(_service.GetAll());

        [HttpPost]
        public IActionResult AddToWishlist(WishlistItemDto item)
        {
            _service.Add(item);
            return Ok();
        }

        [HttpDelete("{productId}")]
        public IActionResult RemoveFromWishlist(int productId)
        {
            _service.Remove(productId);
            return NoContent();
        }
    }
}
