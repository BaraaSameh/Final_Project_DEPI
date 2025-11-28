using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderShippingRepository _orderShippingRepository;

        public ShippingController(
            IShippingService shippingService,
            IOrderRepository orderRepository,
            IOrderShippingRepository orderShippingRepository)
        {
            _shippingService = shippingService;
            _orderRepository = orderRepository;
            _orderShippingRepository = orderShippingRepository;
        }

        /// <summary>
        /// Get all shippings.
        /// </summary>
        /// <response code="200">Shippings retrieved.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ShippingDto>>> GetAllShippings()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("client"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Client" });
            }

            try
            {

                var shippings = await _shippingService.GetAllShippingsAsync();

                var shippingDtos = shippings.Select(s => new ShippingDto
                {
                    ShippingID = s.ShippingID,
                    CompanyName = s.CompanyName,
                    TrackingNumber = s.TrackingNumber,
                    ShippingStatus = s.ShippingStatus,
                    EstimatedDelivery = s.EstimatedDelivery,
                    OrderCount = s.OrderShippings?.Count ?? 0,
                    Orders = s.OrderShippings?.Select(os => new OrderInfoDto
                    {
                        OrderID = os.Order.OrderID,
                        OrderNo = os.Order.OrderNo,
                        TotalAmount = os.Order.TotalAmount,
                        OrderStatus = os.Order.OrderStatus,
                        OrderDate = os.Order.OrderDate
                    }).ToList() ?? new List<OrderInfoDto>()
                }).ToList();

                return Ok(shippingDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving shippings" });
            }
        }

        /// <summary>
        /// Get a shipping by ID.
        /// </summary>
        /// <param name="id">Shipping ID.</param>
        /// <response code="200">Shipping retrieved.</response>
        /// <response code="400">Invalid ID.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="404">Shipping not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShippingDto>> GetShippingById(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid shipping ID" });

                var shipping = await _shippingService.GetShippingByIdAsync(id);

                if (shipping == null)
                    return NotFound(new { message = $"Shipping with ID {id} not found" });

                var shippingDto = new ShippingDto
                {
                    ShippingID = shipping.ShippingID,
                    CompanyName = shipping.CompanyName,
                    TrackingNumber = shipping.TrackingNumber,
                    ShippingStatus = shipping.ShippingStatus,
                    EstimatedDelivery = shipping.EstimatedDelivery,
                    OrderCount = shipping.OrderShippings?.Count ?? 0,
                    Orders = shipping.OrderShippings?.Select(os => new OrderInfoDto
                    {
                        OrderID = os.Order.OrderID,
                        OrderNo = os.Order.OrderNo,
                        TotalAmount = os.Order.TotalAmount,
                        OrderStatus = os.Order.OrderStatus,
                        OrderDate = os.Order.OrderDate
                    }).ToList() ?? new List<OrderInfoDto>()
                };

                return Ok(shippingDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the shipping" });
            }
        }

        /// <summary>
        /// Create a new shipping.
        /// </summary>
        /// <param name="createDto">Shipping creation DTO.</param>
        /// <response code="201">Shipping created.</response>
        /// <response code="400">Invalid data.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShippingDto>> CreateShipping([FromBody] CreateShippingDto createDto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newShipping = new Shipping
                {
                    CompanyName = createDto.CompanyName,
                    TrackingNumber = createDto.TrackingNumber,
                    ShippingStatus = createDto.ShippingStatus,
                    EstimatedDelivery = DateTime.UtcNow.AddDays(createDto.EstimatedDeliveryDays)
                };

                var createdShipping = await _shippingService.CreateShippingAsync(newShipping);

                // Link orders to shipping if OrderIDs provided
                if (createDto.OrderIDs != null && createDto.OrderIDs.Any())
                {
                    foreach (var orderId in createDto.OrderIDs)
                    {
                        var order = await _orderRepository.GetByIdAsync(orderId);
                        if (order != null)
                        {
                            await _orderShippingRepository.CreateAsync(new OrderShipping
                            {
                                OrderID = orderId,
                                ShippingID = createdShipping.ShippingID
                            });
                        }
                    }
                }

                var shippingDto = new ShippingDto
                {
                    ShippingID = createdShipping.ShippingID,
                    CompanyName = createdShipping.CompanyName,
                    TrackingNumber = createdShipping.TrackingNumber,
                    ShippingStatus = createdShipping.ShippingStatus,
                    EstimatedDelivery = createdShipping.EstimatedDelivery,
                    OrderCount = createDto.OrderIDs?.Count ?? 0,
                    Orders = new List<OrderInfoDto>()
                };

                return CreatedAtAction(nameof(GetShippingById),
                    new { id = shippingDto.ShippingID }, shippingDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the shipping" });
            }
        }

        /// <summary>
        /// Update an existing shipping.
        /// </summary>
        /// <param name="id">Shipping ID.</param>
        /// <param name="updateDto">Shipping update DTO.</param>
        /// <response code="200">Shipping updated.</response>
        /// <response code="400">Invalid data or ID mismatch.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="404">Shipping not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShippingDto>> UpdateShipping(int id, [FromBody] UpdateShippingDto updateDto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                if (id != updateDto.ShippingID)
                    return BadRequest(new { message = "ID mismatch between route and body" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingShipping = await _shippingService.GetShippingByIdAsync(id);

                if (existingShipping == null)
                    return NotFound(new { message = $"Shipping with ID {id} not found" });

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.CompanyName))
                    existingShipping.CompanyName = updateDto.CompanyName;

                if (!string.IsNullOrWhiteSpace(updateDto.TrackingNumber))
                    existingShipping.TrackingNumber = updateDto.TrackingNumber;

                if (!string.IsNullOrWhiteSpace(updateDto.ShippingStatus))
                    existingShipping.ShippingStatus = updateDto.ShippingStatus;

                if (updateDto.EstimatedDeliveryDays.HasValue)
                    existingShipping.EstimatedDelivery = DateTime.UtcNow.AddDays(updateDto.EstimatedDeliveryDays.Value);

                // Update orders if OrderIDs provided
                if (updateDto.OrderIDs != null)
                {
                    // Remove existing order-shipping relationships
                    await _orderShippingRepository.RemoveByShippingIdAsync(id);

                    // Add new order-shipping relationships
                    foreach (var orderId in updateDto.OrderIDs)
                    {
                        var order = await _orderRepository.GetByIdAsync(orderId);
                        if (order != null)
                        {
                            await _orderShippingRepository.CreateAsync(new OrderShipping
                            {
                                OrderID = orderId,
                                ShippingID = id
                            });
                        }
                    }
                }

                var updatedShipping = await _shippingService.UpdateShippingAsync(existingShipping);

                if (updatedShipping == null)
                    return NotFound(new { message = $"Shipping with ID {id} not found" });

                var shippingDto = new ShippingDto
                {
                    ShippingID = updatedShipping.ShippingID,
                    CompanyName = updatedShipping.CompanyName,
                    TrackingNumber = updatedShipping.TrackingNumber,
                    ShippingStatus = updatedShipping.ShippingStatus,
                    EstimatedDelivery = updatedShipping.EstimatedDelivery,
                    OrderCount = updatedShipping.OrderShippings?.Count ?? 0,
                    Orders = updatedShipping.OrderShippings?.Select(os => new OrderInfoDto
                    {
                        OrderID = os.Order.OrderID,
                        OrderNo = os.Order.OrderNo,
                        TotalAmount = os.Order.TotalAmount,
                        OrderStatus = os.Order.OrderStatus,
                        OrderDate = os.Order.OrderDate
                    }).ToList() ?? new List<OrderInfoDto>()
                };

                return Ok(shippingDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the shipping" });
            }
        }

        /// <summary>
        /// Update shipping status (patch).
        /// </summary>
        /// <param name="id">Shipping ID.</param>
        /// <param name="statusDto">Status update DTO.</param>
        /// <response code="204">Status updated.</response>
        /// <response code="400">Invalid data.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="404">Shipping not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPatch("{id}/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateShippingStatus(int id, [FromBody] UpdateShippingStatusDto statusDto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });

            }

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _shippingService.UpdateShippingStatus(id, statusDto.ShippingStatus);

                return NoContent();
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the shipping status" });
            }
        }

    }
}


