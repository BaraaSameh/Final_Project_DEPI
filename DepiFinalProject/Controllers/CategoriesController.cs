using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all categories, including:
        /// - Name  
        /// - Description  
        /// - Icon URL  
        /// - Product count  
        /// </remarks>
        /// <response code="200">Returns the list of categories.</response>
        /// <response code="500">Internal server error.</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Uploads an icon for a category.
        /// </summary>
        /// <remarks>
        /// Admin-only endpoint.  
        /// Replaces the category's existing icon if one already exists.
        /// </remarks>
        /// <param name="categoryId">Category ID.</param>
        /// <param name="file">Image file.</param>
        /// <response code="200">Icon uploaded successfully.</response>
        /// <response code="400">Invalid file.</response>
        /// <response code="403">Unauthorized access.</response>
        /// <response code="404">Category not found.</response>
        [HttpPost("{categoryId}/icon")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadIcon(int categoryId, IFormFile file)
        {
            if (!User.IsInRole("admin") )
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

            try
            {

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is empty." });

                var result = await _categoryService.UploadCategoryIconAsync(categoryId, file);

                if (!result)
                    return NotFound(new { message = $"Category with ID {categoryId} not found" });

                return Ok(new { message = "Icon uploaded successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes the icon of a category.
        /// </summary>
        /// <remarks>
        /// Admin-only endpoint.  
        /// Fails if the category has no icon.
        /// </remarks>
        /// <param name="categoryId">Category ID.</param>
        /// <response code="200">Icon deleted successfully.</response>
        /// <response code="403">Unauthorized access.</response>
        /// <response code="404">Category not found or icon missing.</response>
        [HttpDelete("{categoryId}/icon")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteIcon(int categoryId)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }
            try
            {
                var result = await _categoryService.DeleteCategoryIconAsync(categoryId);

                if (!result)
                    return NotFound(new { message = "Category not found or this category has no icon." });

                return Ok(new { message = "Icon deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific category by ID.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <returns>Category details.</returns>
        /// <response code="200">Category found.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="500">Internal server error.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(new { message = $"Category with ID {id} not found" });

                return Ok(category);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="inputDto">Category input data.</param>
        /// <response code="201">Category created successfully.</response>
        /// <response code="400">Invalid data.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CategoryInputDTO inputDto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }
            

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var categoryDto = await _categoryService.CreateCategoryAsync(inputDto);
                return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.CategoryID }, categoryDto);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500,$"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Updates a category.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="inputDto">Updated data.</param>
        /// <response code="204">Category updated.</response>
        /// <response code="400">Invalid data.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryInputDTO inputDto)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin " });
            }

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var updated = await _categoryService.UpdateCategoryAsync(id, inputDto);
                if (!updated)
                    return NotFound(new { message = $"Category with ID {id} not found" });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <response code="204">Category deleted.</response>
        /// <response code="403">Unauthorized.</response>
        /// <response code="404">Category not found or has products.</response>
        /// <response code="500">Internal error.</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin " });
            }

            try
            {
                var deleted = await _categoryService.DeleteCategoryAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Cannot delete category with ID {id} because it has associated products." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }
    }
}
