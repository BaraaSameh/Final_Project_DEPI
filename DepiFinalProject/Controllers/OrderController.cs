using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DepiFinalProject.Core.DTOs.OrderDto;

namespace DepiFinalProject.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get all orders (admin)
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetAllOrders()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

            try
            {
                var orders = await _orderService.GetAllAsync();
                if (!orders.Any())
                    return NotFound(new { message = "No orders found." });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500,$"An error occurred while fetching orders.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get a single order by its ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, $"An internal error occurred.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get all orders belonging to a specific user
        /// </summary>
        [HttpGet("user/{userId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetUserOrders(int userId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And client" });
            }

            try
            {
                var orders = await _orderService.GetByUserAsync(userId);
                if (!orders.Any())
                    return NotFound(new { message = $"No orders found for user ID {userId}." });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to fetch user orders.:{ex.Message} \n {ex.InnerException}");
            }
        }
        // ========== NEW ENDPOINT - Add this ==========
        /// <summary>
        /// Create an order from the current user's cart (Checkout)
        /// </summary>
        /// <returns>Created order details</returns>
        [HttpPost("checkout")]
        [Authorize]
        [ProducesResponseType(typeof(OrderResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDTO>> CheckoutFromCart()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And client" });
            }
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var order = await _orderService.CreateOrderFromCartAsync(userId);

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = order.OrderID },
                    new
                    {
                        message = "Order placed successfully. Your cart has been cleared.",
                        order = order
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create order from cart.:{ex.Message} \n {ex.InnerException}");
            }
        }
        /// <summary>
        /// Create a new order
        /// </summary>
        //[HttpPost]
        //[Authorize(Roles = "admin,client")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<OrderResponseDTO>> CreateOrder([FromBody] CreateOrderDTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        var order = await _orderService.CreateAsync(dto);
        //        return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderID }, order);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Failed to create the order.:{ex.Message} \n {ex.InnerException}");
        //    }
        //}

        /// <summary>
        /// Update an order’s status
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponseDTO>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

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
                return StatusCode(500, $"Failed to update order status.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CancelOrder(int id)
        {
            try
            {
                var success = await _orderService.CancelAsync(id);
                if (!success)
                    return BadRequest(new { message = "Order cannot be cancelled or does not exist." });

                return Ok(new { message = "Order cancelled successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to cancel the order:{ex.Message} \n {ex.InnerException}");
            }
        }
    }
}
