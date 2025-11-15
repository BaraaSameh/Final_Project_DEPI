using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.DTOs.OrderDto;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {

        protected readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [Authorize(Roles = "admin,client,seller")]

        [HttpGet] // GET: api/orders - Get all orders
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                if (!orders.Any())
                    return NotFound(new { message = "No orders found." });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching orders.", details = ex.Message });
            }
        }
        [Authorize(Roles = "admin,client,seller")]

        [HttpGet("{id:int}")] // GET: api/orders/{id} - Get order by ID
        public async Task<ActionResult<OrderDetailsDTO>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetById(id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }
        [Authorize(Roles = "admin,client")]

        [HttpGet("user/{userId:int}")] // GET: api/orders/user/{userId} - Get user orders
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetUserOrders(int userId)
        {
            try
            {
                var orders = await _orderService.GetByUserAsync(userId);
                if (!orders.Any())
                    return NotFound(new { message = $"No orders found for user ID {userId}." });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching user orders.", details = ex.Message });
            }
        }

        [Authorize(Roles = "admin,client")]
        [HttpPost] // POST: api/orders - Create new order
        public async Task<ActionResult<OrderResponseDTO>> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = await _orderService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderID }, order);
            }
            catch (InvalidOperationException ex)
            {
                // For invalid product ID or stock issue
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)  // Catches validation exceptions
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create order.", details = ex.Message });
            }
        }

        [Authorize(Roles = "admin,seller")]
        [HttpPut("{id:int}")] // PUT: api/orders/{id} - Update order status
        public async Task<ActionResult<OrderResponseDTO>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = await _orderService.UpdateStatusAsync(id, dto);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update order status.", details = ex.Message });
            }
        }

        [Authorize(Roles = "admin,client,seller")]
        [HttpDelete("{id:int}")] // DELETE: api/orders/{id} - Cancel order
        public async Task<ActionResult> CancelOrder(int id)
        {
            try
            {
                var result = await _orderService.CancelAsync(id);
                if (!result)
                    return BadRequest(new { message = "Order not found or cannot be cancelled." });
                return Ok(new { message = "Order cancelled successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to cancel order.", details = ex.Message });
            }
        }

    }
}
