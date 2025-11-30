using DepiFinalProject.Core.Commmon.Pagination;
using DepiFinalProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DepiFinalProject.Core.DTOs.ProductDTO;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Services;

namespace DepiFinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IFlashSaleService _flashSaleService;
        public ProductController(IProductService productService,IFlashSaleService flashSaleService)
        {
            _productService = productService;
            _flashSaleService = flashSaleService;
        }
        
/// <summary>
/// Get paginated products with optional category filter (NEW ENDPOINT)
/// </summary>
/// <param name="parameters">Pagination and filter parameters</param>
/// <remarks>
/// Returns paginated list of products with metadata.
/// 
/// Query Parameters:
/// - pageNumber: Page number (default: 1, min: 1)
/// - pageSize: Items per page (default: 10, min: 1, max: 100)
/// - categoryID: Optional category filter
/// 
/// Sample requests:
/// - GET /api/product/paginated (first page, 10 items)
/// - GET /api/product/paginated?pageNumber=2&pageSize=20
/// - GET /api/product/paginated?categoryID=5
/// - GET /api/product/paginated?pageNumber=1&pageSize=50&categoryID=3
/// </remarks>
/// <response code="200">Returns paginated products successfully</response>
/// <response code="400">If an error occurs</response>
[AllowAnonymous]
[HttpGet("paginated")]
[ProducesResponseType(typeof(SuccessResponse<PagedResult<ProductResponseDTO>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<PagedResult<ProductResponseDTO>>> GetProductsPaginated(
    [FromQuery] ProductFilterParameters parameters)
{
    try
    {
        var result = await _productService.GetProductsAsync(parameters);

        return Ok(new SuccessResponse<PagedResult<ProductResponseDTO>>
        {
            Success = true,
            Message = $"Retrieved {result.Data.Count} products from page {result.PageNumber} of {result.TotalPages}",
            Data = result
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new ErrorResponse
        {
            Success = false,
            Message = "An error occurred while fetching paginated products",
            Error = ex.Message,
            Details = ex.InnerException?.Message
        });
    }
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
        [AllowAnonymous]
        [HttpGet]
       // [Authorize(Roles = "admin,client,seller")]
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
        [AllowAnonymous]
        [HttpGet("{id:int}")]
       // [Authorize(Roles = "admin,client,seller")]
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
        [AllowAnonymous]
        [HttpGet("category/{categoryId:int}")]
      //  [Authorize(Roles = "admin,client,seller")]
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
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetMyProducts()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

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
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<ProductResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetByUserId(int userId)
        {
            if (!User.IsInRole("admin"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin" });
            }

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
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<ProductResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ProductResponseDTO>> Create([FromBody] CreateProductDTO dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
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
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<ProductResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ProductResponseDTO>> UpdateProduct(int id, [FromBody] UpdateProductDTO dto)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

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
        /// Delete a specific image from a product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="imageId">The ID of the image to delete.</param>
        /// <returns>Result of deletion operation.</returns>
        /// <response code="200">Image deleted successfully.</response>
        /// <response code="404">Image or product not found.</response>
        /// <response code="401">Unauthorized if user does not have seller/admin role.</response>
        [HttpDelete("{productId}/images/{imageId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteImage(int productId, int imageId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

            bool result = await _productService.DeleteImageAsync(productId, imageId);

            if (!result)
                return NotFound(new { message = "Image not found." });

            return Ok(new { message = "Image deleted successfully." });
        }

        /// <summary>
        /// Add image URLs to a specific product (Cloudinary URLs).
        /// Only admins and sellers can perform this action.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="imageUrls">List of image URLs to add.</param>
        /// <returns>Result of the operation.</returns>
        /// <response code="200">Images added successfully.</response>
        /// <response code="400">No URLs provided.</response>
        /// <response code="401">Unauthorized if user is not seller/admin.</response>
        [HttpPost("{productId}/images")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddImages(int productId, [FromForm] List<IFormFile> images)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

            if (images == null || images.Count == 0)
                return BadRequest(new { message = "No images provided." });
             bool success = await _productService.AddImagesAsync(productId, images);

            if (!success)
                return NotFound(new { message = "Product not found." });

            return Ok(new { message = "Images added successfully." });
        }


        /// <summary>
        /// Delete all images associated with a product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>Result of deletion operation.</returns>
        /// <response code="200">All images deleted successfully.</response>
        /// <response code="404">No images found for the product.</response>
        /// <response code="401">Unauthorized if user does not have seller/admin role.</response>
        [HttpDelete("{productId}/images")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAllImages(int productId)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

            bool result = await _productService.DeleteAllImagesAsync(productId);

            if (!result)
                return NotFound(new { message = "No images found." });

            return Ok(new { message = "All images deleted successfully." });
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
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!User.IsInRole("admin") && !User.IsInRole("seller"))
            {
                return StatusCode(403, new { Error = "Only Allowed To Admin And Seller" });
            }

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
        /// <summary>
        /// Adds a product to a flash sale.
        /// </summary>
        /// <remarks>
        /// Only accessible by Admins.  
        /// Requires a valid Flash Sale ID inside the request body.
        /// </remarks>
        /// <param name="id">The ID of the product to add.</param>
        /// <param name="dto">DTO containing the Flash Sale ID and discount settings.</param>
        /// <response code="200">Product added to flash sale successfully.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="404">Product or flash sale not found.</response>
        /// <response code="401">Unauthorized access.</response>
        [HttpPost("{id}/flashsale")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddToFlashSale(int id, [FromBody] AddProductToFlashSaleRequest dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 1️⃣ Validate product exists
                var product = await _productService.GetById(id);
                if (product == null)
                    return NotFound(new { message = "Product not found" });

                // 2️⃣ Validate flash sale exists
                var flashSale = await _flashSaleService.GetByIdAsync(dto.FlashSaleID);
                if (flashSale == null)
                    return NotFound(new { message = "Flash sale not found" });
                if( dto.StartDate < flashSale.StartDate || dto.EndDate > flashSale.EndDate)
                {
                    return BadRequest(new { message = "Product flash sale dates must be within the flash sale period." });
                }
               

                // 3️⃣ Map Request → Internal DTO (Full DTO)
                var newFlashSaleItem = new AddProductToFlashSaleDto
                {
                    ProductID = id,
                    ProductName = product.ProductName,
                    OriginalPrice = product.Price,
                    FlashSaleID = dto.FlashSaleID,
                    Title = flashSale.Title,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    MaxUsers = dto.MaxUsers,
                    ProductCount = dto.ProductCount,
                    DiscountedPrice = dto.DiscountedPrice,
                    StockLimit = dto.StockLimit,
                    CreatedAt = DateTime.UtcNow,

                    // Auto-calc IsActive
                    IsActive = DateTime.UtcNow >= dto.StartDate &&
                               DateTime.UtcNow <= dto.EndDate &&
                               (dto.MaxUsers == null || dto.MaxUsers > 0)
                };

                // 4️⃣ Save using service
                var added = await _productService.AddProductToFlashSaleAsync(id, newFlashSaleItem);

                if (!added)
                    return NotFound(new { message = "Could not add product to flash sale." });

                return Ok(new { message = "Product added to flash sale successfully." });
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
        /// Retrieves all products currently assigned to flash sales.
        /// </summary>
        /// <response code="200">List of all flash sale products.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("flashsale")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllFlashSaleProducts()
        {
            var result = await _productService.GetAllFlashSaleProductsAsync();
            return Ok(result);
        }


        /// <summary>
        /// Retrieves all products assigned to a specific flash sale.
        /// </summary>
        /// <param name="flashSaleId">The ID of the flash sale.</param>
        /// <response code="200">Products retrieved successfully.</response>
        /// <response code="404">Flash sale not found or contains no products.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("flashsale/{flashSaleId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProductsByFlashSaleId(int flashSaleId)
        {
            var result = await _productService.GetProductsByFlashSaleIdAsync(flashSaleId);

            if (result == null || result.Count == 0)
                return NotFound(new { message = "No products found for this flash sale." });

            return Ok(result);
        }
         
        /// <summary>
        /// Removes a product from a flash sale.
        /// </summary>
        /// <remarks>Only Admins can perform this action.</remarks>
        /// <param name="id">Product ID.</param>
        /// <param name="flashSaleId">Flash sale ID.</param>
        /// <response code="200">Product removed successfully.</response>
        /// <response code="404">Product was not part of this flash sale.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpDelete("{id}/flashsale/{flashSaleId}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFromFlashSale(int id, int flashSaleId)
        {
            try
            {
                if (id <= 0 || flashSaleId <= 0)
                {
                    return BadRequest(new { message = "Invalid product ID or flash sale ID" });
                }
                var result = await _productService.RemoveProductFromFlashSaleAsync(id, flashSaleId);

                if (!result)
                    return NotFound(new { message = "Product not found in this flash sale" });

                return Ok(new { message = "Product removed from flash sale successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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