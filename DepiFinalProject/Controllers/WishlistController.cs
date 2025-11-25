using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out userId);
        }

        /// <summary>
        /// Get all wishlist items for the logged-in user.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(WishlistResponseDto), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WishlistResponseDto>> GetWishlist()
        {
            if (!User.IsInRole("admin")|| !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var items = await _wishlistService.GetAllAsync(userId);

            return Ok(new WishlistResponseDto
            {
                UserId = userId,
                Items = items
            });
        }

        /// <summary>
        /// Get a specific wishlist item by product ID.
        /// </summary>
        [Authorize]
        [HttpGet("{productId:int}")]
        [ProducesResponseType(typeof(WishlistItemDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WishlistItemDto>> GetWishlistItem(int productId)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var item = await _wishlistService.GetByProductIdAsync(userId, productId);

            return item is null
                ? NotFound(new { Message = $"Product {productId} not found in wishlist." })
                : Ok(item);
        }

        /// <summary>
        /// Add a product to the wishlist.
        /// </summary>
        [Authorize]
        [HttpPost("{productId:int}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var item = await _wishlistService.AddAsync(userId, productId);

            return CreatedAtAction(nameof(GetWishlistItem),
                new { productId },
                item);
        }

        /// <summary>
        /// Remove a product from the wishlist.
        /// </summary>
        [Authorize]
        [HttpDelete("{productId:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var removed = await _wishlistService.RemoveAsync(userId, productId);

            return removed
                ? NoContent()
                : NotFound(new { Message = $"Product {productId} not found in wishlist." });
        }

        /// <summary>
        /// Clear the entire wishlist.
        /// </summary>
        [Authorize]
        [HttpDelete]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearWishlist()
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var cleared = await _wishlistService.ClearAsync(userId);

            return cleared
                ? NoContent()
                : BadRequest(new { Message = "Wishlist is already empty." });
        }
    }
}
