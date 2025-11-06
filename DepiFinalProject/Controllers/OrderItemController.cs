using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.DTOs.OrderDto;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderItemController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItemResponseDTO>>> GetOrderItems(int orderId)
        {
            try
            {
                var orderItems = await _orderService.GetOrderItemsAsync(orderId);
                return Ok(orderItems);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching order items.", details = ex.Message });
            }
        }

        // POST: api/orders/{orderId}/items
        [HttpPost]
        public async Task<ActionResult<OrderItemResponseDTO>> AddOrderItem(int orderId, [FromBody] AddOrderItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var orderItem = await _orderService.AddOrderItemAsync(orderId, dto);
                return CreatedAtAction(
                    nameof(GetOrderItems),
                    new { orderId = orderId },
                    orderItem
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add item to order.", details = ex.Message });
            }
        }
    }
}