using DepiFinalProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        /// <summary>
        /// Retrieves all products from the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all available products with their basic information including category and seller details.
        /// </remarks>
        /// <response code="200">Returns the list of products successfully</response>
        /// <response code="400">If an error occurs while fetching products</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have the required role</response>
        [HttpGet]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return Ok(new SuccessResponse<IEnumerable<ProductResponseDTO>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while fetching products",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Retrieves a specific product by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the product</param>
        /// <remarks>
        /// Returns detailed information about a product including reviews and ratings.
        /// </remarks>
        /// <response code="200">Returns the product details successfully</response>
        /// <response code="400">If an error occurs while fetching the product</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have the required role</response>
        /// <response code="404">If the product with the specified ID is not found</response>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(SuccessResponse<ProductDetailsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDetailsDTO>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Invalid product ID",
                    Error = "Product ID must be greater than 0"
                });
            }

            try
            {
                var product = await _productService.GetById(id);

                if (product == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = $"Product not found",
                        Error = $"No product exists with ID {id}"
                    });
                }

                return Ok(new SuccessResponse<ProductDetailsDTO>
                {
                    Success = true,
                    Message = "Product retrieved successfully",
                    Data = product
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Product not found",
                    Error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "An error occurred while fetching the product",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all products in a specific category
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category</param>
        /// <remarks>
        /// Returns a list of all products belonging to the specified category.
        /// </remarks>
        /// <response code="200">Returns the list of products in the category successfully</response>
        /// <response code="400">If an error occurs while fetching products</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have the required role</response>
        /// <response code="404">If no products are found in the specified category</response>
        [HttpGet("category/{categoryId:int}")]
        [Authorize(Roles = "admin,client,seller")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetByCategory(int categoryId)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Invalid category ID",
                    Error = "Category ID must be greater than 0"
                });
            }

            try
            {
                var products = await _productService.GetByCategoryAsync(categoryId);

                if (products == null || !products.Any())
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "No products found",
                        Error = $"No products exist in category with ID {categoryId}"
                    });
                }

                return Ok(new SuccessResponse<IEnumerable<ProductResponseDTO>>
                {
                    Success = true,
                    Message = $"Found {products.Count()} product(s) in the category",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Failed to fetch products by category",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all products created by the currently authenticated user
        /// </summary>
        /// <remarks>
        /// Returns a list of products that belong to the logged-in seller or admin user.
        /// The user ID is automatically extracted from the authentication token.
        /// </remarks>
        /// <response code="200">Returns the list of user's products successfully (may be empty)</response>
        /// <response code="400">If an error occurs while fetching products</response>
        /// <response code="401">If the user is not authenticated or user ID is invalid</response>
        /// <response code="403">If the user doesn't have seller or admin role</response>
        [HttpGet("my-products")]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetMyProducts()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ErrorResponse
                    {
                        Success = false,
                        Message = "Authentication failed",
                        Error = "User is not authenticated or user ID is invalid"
                    });
                }

                var products = await _productService.GetByUserIdAsync(userId);
                var productList = products?.ToList() ?? new List<ProductResponseDTO>();

                return Ok(new SuccessResponse<IEnumerable<ProductResponseDTO>>
                {
                    Success = true,
                    Message = productList.Any()
                        ? $"Found {productList.Count} product(s)"
                        : "No products found for this user",
                    Data = productList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Failed to fetch user products",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all products created by a specific user (Admin only)
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <remarks>
        /// Admin-only endpoint that returns all products belonging to any user in the system.
        /// </remarks>
        /// <response code="200">Returns the list of user's products successfully</response>
        /// <response code="400">If an error occurs while fetching products</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have admin role</response>
        /// <response code="404">If no products are found for the specified user</response>
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Invalid user ID",
                    Error = "User ID must be greater than 0"
                });
            }

            try
            {
                var products = await _productService.GetByUserIdAsync(userId);

                if (products == null || !products.Any())
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "No products found",
                        Error = $"No products exist for user with ID {userId}"
                    });
                }

                return Ok(new SuccessResponse<IEnumerable<ProductResponseDTO>>
                {
                    Success = true,
                    Message = $"Found {products.Count()} product(s) for user {userId}",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Failed to fetch products by user",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Creates a new product
        /// </summary>
        /// <param name="dto">The product creation data</param>
        /// <remarks>
        /// Creates a new product in the system. The seller ID is automatically assigned from the authenticated user.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/product
        ///     {
        ///         "categoryId": 1,
        ///         "name": "Wireless Mouse",
        ///         "description": "Ergonomic wireless mouse with USB receiver",
        ///         "price": 29.99,
        ///         "stock": 100,
        ///         "imageUrl": "https://example.com/images/mouse.jpg"
        ///     }
        /// 
        /// </remarks>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">If the product data is invalid or an error occurs</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have seller or admin role</response>
        /// <response code="422">If validation fails</response>
        [HttpPost]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(typeof(SuccessResponse<ProductResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ProductResponseDTO>> Create([FromBody] CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return UnprocessableEntity(new ValidationErrorResponse
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            try
            {
                var product = await _productService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = product.ProductID },
                    new SuccessResponse<ProductResponseDTO>
                    {
                        Success = true,
                        Message = "Product created successfully",
                        Data = product
                    });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ErrorResponse
                {
                    Success = false,
                    Message = "Authorization failed",
                    Error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Failed to create product",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing product
        /// </summary>
        /// <param name="id">The unique identifier of the product to update</param>
        /// <param name="dto">The product update data</param>
        /// <remarks>
        /// Updates product information. Only the product owner or an admin can update a product.
        /// All fields are optional - only provided fields will be updated.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/product/5
        ///     {
        ///         "name": "Updated Product Name",
        ///         "price": 39.99,
        ///         "stock": 150
        ///     }
        /// 
        /// </remarks>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">If an error occurs during update</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to update this product</response>
        /// <response code="404">If the product is not found</response>
        /// <response code="422">If validation fails</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(typeof(SuccessResponse<ProductResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ProductResponseDTO>> UpdateProduct(int id, [FromBody] UpdateProductDTO dto)
        {
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Invalid product ID",
                    Error = "Product ID must be greater than 0"
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return UnprocessableEntity(new ValidationErrorResponse
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            try
            {
                var updated = await _productService.UpdateAsync(id, dto);

                if (updated == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "Product not found",
                        Error = $"No product exists with ID {id}"
                    });
                }

                return Ok(new SuccessResponse<ProductResponseDTO>
                {
                    Success = true,
                    Message = "Product updated successfully",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Success = false,
                    Message = "Access denied",
                    Error = "You are not authorized to update this product"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Product not found",
                    Error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Failed to update product",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id">The unique identifier of the product to delete</param>
        /// <remarks>
        /// Permanently deletes a product from the system. Only the product owner or an admin can delete a product.
        /// </remarks>
        /// <response code="200">Product deleted successfully</response>
        /// <response code="400">If an error occurs during deletion</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to delete this product</response>
        /// <response code="404">If the product is not found</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin,seller")]
        [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Invalid product ID",
                    Error = "Product ID must be greater than 0"
                });
            }

            try
            {
                var deleted = await _productService.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new ErrorResponse
                    {
                        Success = false,
                        Message = "Product not found",
                        Error = $"No product exists with ID {id}"
                    });
                }

                return Ok(new SuccessResponse<object>
                {
                    Success = true,
                    Message = "Product deleted successfully",
                    Data = new { ProductId = id, DeletedAt = DateTime.UtcNow }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Success = false,
                    Message = "Access denied",
                    Error = "You are not authorized to delete this product"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Message = "Error deleting product",
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }
    }

    // Response Models for Swagger Documentation
    public class SuccessResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string? Details { get; set; }
    }

    public class ValidationErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}