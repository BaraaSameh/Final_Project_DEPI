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
        /// Get the current user's cart with all items.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
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
        /// Get a specific item in the current user's cart.
        /// </summary>
        [HttpGet("{productId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartItemDto>> GetCartItem(int productId)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
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
        /// Add a product to the current user's cart.
        /// </summary>
        [HttpPost("{productId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
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
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the item to the cart.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Update the quantity of a specific product in the cart.
        /// </summary>
        [HttpPut("{productId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateQuantityDto dto)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
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
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the item quantity.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Remove all items from the current user's cart.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart()
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
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
        /// Remove a specific item from the current user's cart.
        /// </summary>
        [HttpDelete("{productId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            if (!User.IsInRole("admin") || !User.IsInRole("client"))
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
