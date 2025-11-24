using DepiFinalProject.DTOs;
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

        // GET: api/flashsales
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var flashSales = await _service.GetAllAsync();
            return Ok(flashSales);
        }

        // GET: api/flashsales/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            var flashSale = await _service.GetByIdAsync(id);

            if (flashSale == null)
                return NotFound(new { message = "Flash sale not found" });

            return Ok(flashSale);
        }

        // POST: api/flashsales
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateFlashSaleDto dto)
        {
            if (!User.IsInRole("admin") )
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { message = "End date must be after start date" });

            var created = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(Get), new { id = created.FlashSaleID }, created);
        }

        // PUT: api/flashsales/{id}
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFlashSaleDto dto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, dto);

            if (updated == null)
                return NotFound(new { message = "Flash sale not found" });

            return Ok(updated);
        }

        // DELETE: api/flashsales/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new { message = "Flash sale not found" });

            return Ok(new { message = "Flash sale deleted successfully" });
        }
    }
}