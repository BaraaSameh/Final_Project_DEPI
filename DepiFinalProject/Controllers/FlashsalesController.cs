using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DepiFinalProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FlashSalesController : ControllerBase
    {
        private readonly IFlashSaleService _service;

        public FlashSalesController(IFlashSaleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all flash sales.
        /// </summary>
        /// <response code="200">Returns all flash sales.</response>
        /// <response code="500">Internal server error.</response>   
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var flashSales = await _service.GetAllAsync();
                return Ok(flashSales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves a flash sale by ID.
        /// </summary>
        /// <param name="id">Flash sale ID.</param>
        /// <response code="200">Flash sale retrieved successfully.</response>
        /// <response code="404">Flash sale not found.</response>
        /// <response code="500">Internal server error.</response>   
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var flashSale = await _service.GetByIdAsync(id);

                if (flashSale == null)
                    return NotFound(new { message = "Flash sale not found" });

                return Ok(flashSale);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// Creates a new flash sale.
        /// </summary>
        /// <param name="dto">Flash sale creation payload.</param>
        /// <response code="201">Flash sale created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="403">Not authorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateFlashSaleDto dto)
        {
            if (!User.IsInRole("admin") )
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if(dto.StartDate< DateTime.UtcNow)
                    return BadRequest(new { message = "Start date must be in the future" });
                if (dto.EndDate <= dto.StartDate)
                    return BadRequest(new { message = "End date must be after start date" });
                var userId = int.Parse(User.FindFirst("userId").Value);
                dto.UserID = userId;
                var created = await _service.CreateAsync(dto);

                return CreatedAtAction(nameof(Get), new { id = created.FlashSaleID }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing flash sale.
        /// </summary>
        /// <param name="id">Flash sale ID.</param>
        /// <param name="dto">Updated flash sale data.</param>
        /// <response code="200">Flash sale updated successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="403">Not authorized.</response>
        /// <response code="404">Flash sale not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFlashSaleDto dto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if(dto.StartDate.HasValue && dto.StartDate < DateTime.UtcNow)
                    return BadRequest(new { message = "Start date must be in the future" });
                if(dto.EndDate.HasValue && dto.StartDate.HasValue && dto.EndDate <= dto.StartDate)
                    return BadRequest(new { message = "End date must be after start date" });
                var updated = await _service.UpdateAsync(id, dto);

                if (updated == null)
                    return NotFound(new { message = "Flash sale not found" });

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a flash sale.
        /// </summary>
        /// <param name="id">Flash sale ID.</param>
        /// <response code="200">Flash sale deleted successfully.</response>
        /// <response code="403">Not authorized.</response>
        /// <response code="404">Flash sale not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }
            try
            {
                var deleted = await _service.DeleteAsync(id);

                if (!deleted)
                    return NotFound(new { message = "Flash sale not found" });

                return Ok(new { message = "Flash sale deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}