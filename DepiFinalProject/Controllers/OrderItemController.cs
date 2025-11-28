using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.Core.DTOs.OrderDto;

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

        /// <summary>
        /// Retrieves all items belonging to an order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <response code="200">Items retrieved successfully.</response>
        /// <response code="400">Invalid order ID.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(IEnumerable<OrderItemResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Adds a new item to an order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>
        /// <param name="dto">Order item details.</param>
        /// <response code="201">Item added to order.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Only Admin and Client can add items.</response>
        /// <response code="404">Order not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(OrderItemResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderItemResponseDTO>> AddOrderItem(int orderId, AddOrderItemDTO dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And client" });
            }

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
