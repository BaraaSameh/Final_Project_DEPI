using DepiFinalProject.DTOs;
using DepiFinalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _service;

        public CartController(CartService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<List<CartItemDto>> GetCart() => Ok(_service.GetAll());

        [HttpPost]
        public IActionResult AddToCart(CartItemDto item)
        {
            _service.Add(item);
            return Ok();
        }

        [HttpPut("{productId}")]
        public IActionResult UpdateQuantity(int productId, [FromBody] int quantity)
        {
            _service.UpdateQuantity(productId, quantity);
            return NoContent();
        }

        [HttpDelete("{productId}")]
        public IActionResult RemoveFromCart(int productId)
        {
            _service.Remove(productId);
            return NoContent();
        }
    }
}
