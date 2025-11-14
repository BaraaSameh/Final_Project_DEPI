using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using static DepiFinalProject.DTOs.ProductDTO;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        protected readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet] //api/products
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
                return StatusCode(500, new { message = "An error occurred while fetching products.", details = ex.Message });
            }
        }

        [HttpGet("{id:int}")] //api/products/{id}
        [Authorize(Roles = "admin,client,seller")]
        public async Task<ActionResult<ProductDetailsDTO>> GetById(int id)
        {

            try
            {
                var product = await _productService.GetById(id);
                return Ok(product);
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

        [Authorize(Roles = "admin,client,seller")]
        [HttpGet("category/{categoryId}")] //api/products/category/{categoryId}
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
                return StatusCode(500, new { message = "An error occurred while fetching products by category.", details = ex.Message });
            }

        }

        [HttpPost] //api/products
        [Authorize(Roles = "admin,seller")]
        public async Task<ActionResult<ProductResponseDTO>> Create(CreateProductDTO dto)
        {
            try
            {
                var product = await _productService.CreateAsync(dto);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = product.ProductID },
                    product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create product.", details = ex.Message });
            }
        }

        [HttpPut("{id:int}")] //api/products/{id}
        [Authorize(Roles = "admin,seller")]
        public async Task<ActionResult<ProductResponseDTO>> UpdateProduct(int id, UpdateProductDTO dto)
        {
            try
            {

                var product = await _productService.UpdateAsync(id, dto);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update product.", details = ex.Message });
            }
        }

        [HttpDelete("{id:int}")] //api/products/{id}
        [Authorize(Roles = "admin,seller")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Product not found" });
                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting product.", details = ex.Message });
            }
        }

    }
}
