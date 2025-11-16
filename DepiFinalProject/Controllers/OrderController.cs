using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DepiFinalProject.DTOs.OrderDto;

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
        /// Get all orders (admin, client, seller)
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [Authorize(Roles = "admin,client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                return StatusCode(500, $"Failed to fetch user orders.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,client")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create the order.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Update an order’s status
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
