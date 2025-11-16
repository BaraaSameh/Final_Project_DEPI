using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.DTOs.OrderDto;

namespace DepiFinalProject.Controllers
{
    [Route("api/orders/{orderId}/items")]
    [ApiController]
    [Authorize]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderItemController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(IEnumerable<OrderItemResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<OrderItemResponseDTO>>> GetOrderItems(int orderId)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Invalid order ID." });

            try
            {
                var items = await _orderService.GetOrderItemsAsync(orderId);
                return Ok(items);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to fetch order items.:{ex.Message} \n {ex.InnerException}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin,client")]
        [ProducesResponseType(typeof(OrderItemResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OrderItemResponseDTO>> AddOrderItem(int orderId, AddOrderItemDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var item = await _orderService.AddOrderItemAsync(orderId, dto);

                return CreatedAtAction(nameof(GetOrderItems),
                    new { orderId = orderId },
                    item);
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
                return BadRequest($"Failed to add item to order.:{ex.Message} \n {ex.InnerException}");
            }
        }
    }
}
