using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// Get all categories.
        /// </summary>
        /// <returns>List of categories.</returns>
        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
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
        /// Get a specific category by ID.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category data</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,client,seller")]
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
        /// Create a new category.
        /// </summary>
        /// <param name="inputDto">Category input data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CategoryInputDTO inputDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var categoryDto = await _categoryService.CreateCategoryAsync(inputDto);
                return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.CategoryID }, categoryDto);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500,$"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Update an existing category.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="inputDto">Updated category data</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryInputDTO inputDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
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
        /// Delete a category.
        /// </summary>
        /// <param name="id">Category ID</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryService.DeleteCategoryAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Cannot delete category with ID {id} because it has associated products." });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error deleting product.:{ex.Message} \n {ex.InnerException}");
            }
        }
    }
}
