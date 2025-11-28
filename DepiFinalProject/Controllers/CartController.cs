using System.Security.Claims;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get the current user's ID from claims.
        /// </summary>
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// Retrieves all items in the current user's cart.
        /// </summary>
        /// <remarks>
        /// This endpoint returns the cart items along with total quantity and price.
        /// Only users with roles "admin" or "client" can access it.
        /// </remarks>
        /// <response code="200">Cart retrieved successfully.</response>
        /// <response code="403">Forbidden — user does not have required roles.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            try
            {
                var userId = GetUserId();
                var items = await _cartService.GetAllAsync(userId);

                var response = new CartResponseDto
                {
                    UserId = userId,
                    Items = items,
                    TotalQuantity = items.Sum(i => i.Quantity),
                    TotalPrice = items.Sum(i => i.Quantity * i.Price)
                };

                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the cart.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Retrieves a single specific item in the user's cart.
        /// </summary>
        /// <remarks>
        /// If the product is not found in the cart, a 404 response is returned.
        /// </remarks>
        /// <param name="productId">ID of the product to retrieve.</param>
        /// <response code="200">Item retrieved successfully.</response>
        /// <response code="404">Item not found in the cart.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItemDto>> GetCartItem(int productId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            try
            {
                var userId = GetUserId();
                var item = await _cartService.GetByProductIdAsync(userId, productId);

                if (item == null)
                    return NotFound(new { Message = $"Product {productId} not found in cart." });

                return Ok(item);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the cart item.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Adds a product to the current user's cart.
        /// </summary>
        /// <remarks>
        /// If the product does not exist, a 404 response is returned.
        /// </remarks>
        /// <param name="productId">Product ID to add.</param>
        /// <param name="quantity">Quantity to add (defaults to 1).</param>
        /// <response code="200">Item added successfully.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                var userId = GetUserId();
                var item = new AddToCartRequestDto
                {
                    ProductId = productId,
                    Quantity = quantity > 0 ? quantity : 1
                };

                await _cartService.AddAsync(userId, item);
                var cartItem = await _cartService.GetByProductIdAsync(userId, productId);

                if (cartItem == null)
                    return NotFound(new { Message = $"Product {productId} could not be added." });

                return Ok(new
                {
                    Message = "Item added successfully.",
                    ProductId = productId,
                    Quantity = cartItem.Quantity,
                    TotalPrice = cartItem.Quantity * cartItem.Price
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the item to the cart.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Updates the quantity of a specific item in the user's cart.
        /// </summary>
        /// <remarks>
        /// Quantity must be greater than zero.
        /// </remarks>
        /// <param name="productId">Product ID to update.</param>
        /// <param name="dto">New quantity value.</param>
        /// <response code="200">Quantity updated successfully.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateQuantityDto dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                var userId = GetUserId();
                await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);

                return Ok(new
                {
                    Message = $"Quantity updated to {dto.Quantity} for product {productId}"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the item quantity.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Removes all items from the user's cart.
        /// </summary>
        /// <remarks>
        /// This action deletes all cart entries for the authenticated user.
        /// </remarks>
        /// <response code="200">Cart cleared successfully.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                var userId = GetUserId();
                await _cartService.ClearAsync(userId);
                return Ok(new { Message = "All products removed from your cart." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500,  $"An error occurred while clearing the cart.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Removes a single specific item from the user's cart.
        /// </summary>
        /// <remarks>
        /// If the product is not in the cart, removal still succeeds (idempotent).
        /// </remarks>
        /// <param name="productId">Product ID to remove.</param>
        /// <response code="200">Item removed successfully.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }
            try
            {
                var userId = GetUserId();
                await _cartService.RemoveAsync(userId, productId);

                return Ok(new { Message = $"Product {productId} removed from your cart." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while removing the item from the cart.:{ex.Message} \n {ex.InnerException}");
            }
        }
    }
}
