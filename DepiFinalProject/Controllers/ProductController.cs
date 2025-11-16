using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DepiFinalProject.DTOs.ProductDTO;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while fetching products.:{ex.Message} \n {ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin,client,seller")]
        public async Task<ActionResult<ProductDetailsDTO>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetById(id);

                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while fetching product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        [HttpGet("category/{categoryId:int}")]
        [Authorize(Roles = "admin,client,seller")]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetByCategoryAsync(categoryId);

                if (products == null || !products.Any())
                    return NotFound(new { message = $"No products found for category ID {categoryId}." });

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to fetch products by category.:{ex.Message} \n {ex.InnerException}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin,seller")]
        public async Task<ActionResult<ProductResponseDTO>> Create(CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var product = await _productService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetById),
                    new { id = product.ProductID },
                    product);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create product.:{ex.Message} \n {ex.InnerException}");
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin,seller")]
        public async Task<ActionResult<ProductResponseDTO>> UpdateProduct(int id, UpdateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var updated = await _productService.UpdateAsync(id, dto);

                if (updated == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update product.t.:{ex.Message} \n {ex.InnerException}");
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin,seller")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var deleted = await _productService.DeleteAsync(id);

                if (!deleted)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting product.:{ex.Message} \n {ex.InnerException}");

            }
        }
    }
}
