using DepiFinalProject.DTOs;
<<<<<<< HEAD
using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet]
        public async Task<ActionResult<WishlistResponseDto>> GetWishlist()
        {
            try
            {
                var userId = GetUserId();
                var items = await _wishlistService.GetAllAsync(userId);

                var response = new WishlistResponseDto
                {
                    UserId = userId,
                    Items = items
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving wishlist.", Error = ex.Message });
            }
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<WishlistItemDto>> GetWishlistItem(int productId)
        {
            try
            {
                var userId = GetUserId();
                var item = await _wishlistService.GetByProductIdAsync(userId, productId);

                if (item == null)
                    return NotFound(new { Message = $"Product {productId} not found in wishlist." });

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving item.", Error = ex.Message });
            }
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            try
            {
                var userId = GetUserId();
                await _wishlistService.AddAsync(userId, productId);
                return Ok(new { Message = $"Product {productId} added to wishlist." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding product.", Error = ex.Message });
            }
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            try
            {
                var userId = GetUserId();
                await _wishlistService.RemoveAsync(userId, productId);
                return Ok(new { Message = $"Product {productId} removed from wishlist." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while removing product.", Error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            try
            {
                var userId = GetUserId();
                await _wishlistService.ClearAsync(userId);
                return Ok(new { Message = "Wishlist cleared successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while clearing wishlist.", Error = ex.Message });
            }
=======
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
>>>>>>> 2efc83d (initial user commit)
        }
    }
}
