using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
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

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet]
        [Authorize(Roles = "admin,client")]
        public async Task<ActionResult<CartResponseDto>> GetCart()
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

        [HttpGet("{productId}")]
        [Authorize(Roles = "admin,client")]

        public async Task<ActionResult<CartItemDto>> GetCartItem(int productId)
        {
           
                var userId = GetUserId();
                var item = await _cartService.GetByProductIdAsync(userId, productId);

                if (item == null)
                    return NotFound();

                return Ok(item);
           
            
        }

        [HttpPost("{productid}")]
        [Authorize(Roles = "admin,client")]

        public async Task<IActionResult> AddToCart(int productid, int quantity)
        {
            try {
                var item = new AddToCartRequestDto
                {
                    ProductId = productid,
                    Quantity = quantity > 0 ? quantity : 1
                };
                var userId = GetUserId();
                await _cartService.AddAsync(userId, item);
                var cartitem = await _cartService.GetByProductIdAsync(userId, item.ProductId);

                if (cartitem == null)
                    return NotFound();
                return Ok(new { Message = $"item added to your cart successfully. and your quantity of the the product {item.ProductId} is {cartitem.Quantity} with total price {cartitem.Price * cartitem.Quantity}" });

            }
            catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while adding the item to the cart.", Details = ex.Message });
            }
         }

        [HttpPut("{productId}")]
        [Authorize(Roles = "admin,client")]

        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateQuantityDto dto)
        {
            try {
                var userId = GetUserId();
                await _cartService.UpdateQuantityAsync(userId, productId, dto.Quantity);
                return Ok(new { Message = $"Quantity updated to {dto.Quantity} for product {productId}" });

            }
            catch (Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while updating the item quantity.", Details = ex.Message });
            }
                  }
        [HttpDelete]
        [Authorize(Roles = "admin,client")]

        public async Task<IActionResult> ClearCart()
        {
            try {
                var userId = GetUserId();
                await _cartService.ClearAsync(userId);
                return Ok(new { Message = "All products removed from your cart." });
            }
            catch(Exception ex) { 
                return StatusCode(500, new { Message = "An error occurred while clearing the cart.", Details = ex.Message });
            }
        
        }


        [HttpDelete("{productId}")]
        [Authorize(Roles = "admin,client")]

        public async Task<IActionResult> RemoveItem(int productId)
        {
            try {
                var userId = GetUserId();
                await _cartService.RemoveAsync(userId, productId);
                return Ok();
            }
            catch(Exception ex) {
                return StatusCode(500, new { Message = "An error occurred while removing the item from the cart.", Details = ex.Message });
            }
          
        }
    }
}
