using DepiFinalProject.Data;
using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/categories
        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]

        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/categories/id
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,client,seller")]

        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "admin,seller")]

        public async Task<ActionResult<CategoryDTO>> CreateCategory(CategoryInputDTO inputDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryDto = await _categoryService.CreateCategoryAsync(inputDto);

            return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.CategoryID }, categoryDto);
        }

        // PUT: api/categories/id
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,seller")]

        public async Task<IActionResult> UpdateCategory(int id, CategoryInputDTO inputDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _categoryService.UpdateCategoryAsync(id, inputDto);

            if (!result)
            {
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return NoContent();
        }

        // DELETE: api/categories/id
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,seller")]

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            if (!result)
            {
                return NotFound(new { message = $"Cant Delete...Category with ID {id} has PRODUCTS!" });
            }

            return NoContent();
        }


    }
}
