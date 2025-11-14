using DepiFinalProject.DTOs;
using DepiFinalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
        public ActionResult<List<CartItemDto>> GetCart() => Ok(_service.GetAll());

        [Authorize(Roles = "admin,client")]
        [HttpPost]
        public IActionResult AddToCart(CartItemDto item)
        {
            _service.Add(item);
            return Ok();
        }

        [HttpPut("{productId}")]
        [Authorize(Roles = "admin,client")]
        public IActionResult UpdateQuantity(int productId, [FromBody] int quantity)
        {
            _service.UpdateQuantity(productId, quantity);
            return NoContent();
        }

        [HttpDelete("{productId}")]
        [Authorize(Roles = "admin,client")]
        public IActionResult RemoveFromCart(int productId)
        {
            _service.Remove(productId);
            return NoContent();
        }
    }
}
