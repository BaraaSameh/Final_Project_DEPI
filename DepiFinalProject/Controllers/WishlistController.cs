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
            if (!User.IsInRole("admin")&& !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var items = await _wishlistService.GetAllAsync(userId);

                return Ok(new WishlistResponseDto
                {
                    UserId = userId,
                    Items = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the wishlist for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { Error = "An unexpected error occurred. Please try again later." });
            }
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
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var item = await _wishlistService.GetByProductIdAsync(userId, productId);

                return item is null
                    ? NotFound(new { Message = $"Product {productId} not found in wishlist." })
                    : Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the wishlist item for user {UserId} and product {ProductId}", User.FindFirstValue(ClaimTypes.NameIdentifier), productId);
                return StatusCode(500, new { Error = "An unexpected error occurred. Please try again later." });
            }
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
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var item = await _wishlistService.AddAsync(userId, productId);

                return CreatedAtAction(nameof(GetWishlistItem),
                    new { productId },
                    item);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Invalid input while adding product {ProductId} to wishlist for user {UserId}", productId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Error = argEx.Message });
            }
            catch (KeyNotFoundException notFoundEx)
            {
                _logger.LogWarning(notFoundEx, "Product {ProductId} not found while adding to wishlist for user {UserId}", productId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return NotFound(new { Error = notFoundEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding product {ProductId} to wishlist for user {UserId}", productId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { Error = "An unexpected error occurred. Please try again later." });
            }
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
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var removed = await _wishlistService.RemoveAsync(userId, productId);

                return removed
                    ? NoContent()
                    : NotFound(new { Message = $"Product {productId} not found in wishlist." });
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing product {ProductId} from wishlist for user {UserId}", productId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { Error = "An unexpected error occurred. Please try again later." });
            }
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
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "only Allowed To Admin And Client" });
            }

            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var cleared = await _wishlistService.ClearAsync(userId);

                return cleared
                    ? NoContent()
                    : BadRequest(new { Message = "Wishlist is already empty." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while clearing the wishlist for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { Error = "An unexpected error occurred. Please try again later." });
            }
        }
        /// <summary>
        /// Move all wishlist items to cart.
        /// </summary>
        [Authorize]
        [HttpPost("move-all-to-cart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> MoveAllToCart()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });

            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var moved = await _wishlistService.MoveAllToCartAsync(userId);

                return moved
                    ? Ok(new { Message = "All wishlist items moved to cart." })
                    : BadRequest(new { Message = "Wishlist is empty." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving all wishlist items to cart for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
        /// <summary>
        /// Move specific item from wishlist to cart.
        /// </summary>
        [Authorize]
        [HttpPost("{productId:int}/move-to-cart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> MoveItemToCart(int productId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });

            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized();

                var moved = await _wishlistService.MoveItemToCartAsync(userId, productId);

                return moved
                    ? Ok(new { Message = $"Product {productId} moved to cart." })
                    : NotFound(new { Message = $"Product {productId} not found in wishlist." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving wishlist item {ProductId} to cart for user {UserId}", productId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }

    }
}
