using DepiFinalProject.DTOs;
using DepiFinalProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly WishlistService _service;

        public WishlistController(WishlistService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]

        public ActionResult<List<WishlistItemDto>> GetWishlist() => Ok(_service.GetAll());

        [HttpPost]
        [Authorize(Roles = "admin,client,seller")]
        public IActionResult AddToWishlist(WishlistItemDto item)
        {
            _service.Add(item);
            return Ok();
        }

        [HttpDelete("{productId}")]
        [Authorize(Roles = "admin,client,seller")]

        public IActionResult RemoveFromWishlist(int productId)
        {
            _service.Remove(productId);
            return NoContent();
        }
    }
}
