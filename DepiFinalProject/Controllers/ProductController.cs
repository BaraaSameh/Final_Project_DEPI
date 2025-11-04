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
    public class ProductController : ControllerBase
    {
        protected readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet] //api/products
        public async Task<ActionResult<IEnumerable<ResponseDTO>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")] //api/products/{id}
        public async Task<ActionResult<DetailsDTO>> GetById(int id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });
            return Ok(product);
        }

        [HttpGet("category/{categoryId}")] //api/products/category/{categoryId}
        public async Task<ActionResult<IEnumerable<ResponseDTO>>> GetByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId);
            return Ok(products);
        }

        [HttpPost] //api/products
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> Create(CreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var product = await _productService.CreateAsync(dto);
            if (product == null)
                return BadRequest(new { message = "Failed to create product" });
            return CreatedAtAction(
                nameof(GetById),
                new { id = product.ProductID },
                product);
        }

        [HttpPut("{id:int}")] //api/products/{id}
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> UpdateProduct(int id, UpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var product = await _productService.UpdateAsync(id, dto);
            if (product == null)
                return NotFound(new { message = "Product not found" });
            return Ok(product);
        }

        [HttpDelete("{id:int}")] //api/products/{id}
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Product not found" });
            return NoContent();
        }

    }
}
