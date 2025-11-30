using System.Security.Claims;
using DepiFinalProject.core.DTOs;
using DepiFinalProject.Core.Commmon.Pagination;
using DepiFinalProject.Core.DTOs;
using DepiFinalProject.Core.Interfaces;
using DepiFinalProject.Core.Models;
using DepiFinalProject.Infrastructurenamespace.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaypalServerSdk.Standard.Models;
using static DepiFinalProject.Core.DTOs.ProductDTO;

namespace DepiFinalProject.Services
{
    public class ProductService : IProductService
    {
        protected readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUserRepository _userRepository;
        private readonly IFlashSaleRepository _flashSaleRepository;
        public ProductService(IProductRepository productRepository, IHttpContextAccessor httpContextAccessor,ICloudinaryService cloudinaryService, IUserRepository userRepository, IFlashSaleRepository flashSaleRepository)
        {
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryService = cloudinaryService;
            _userRepository = userRepository;
            _flashSaleRepository = flashSaleRepository;
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Invalid User.");

            return userId;
        }
        public async Task<bool> AddImagesAsync(int productId, List<IFormFile> images)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new Exception("Product not found.");

            int userId = GetCurrentUserId();

            if (product.userid != userId)
                throw new UnauthorizedAccessException("You can only upload images to your own products.");
            List<string> imageUrls = new List<string>();
            List<string> imagepublicid = new List<string>();
            foreach (IFormFile imageFile in images)
            {
                var (url, publicId) = await _cloudinaryService.UploadImageAsync(imageFile);
                imageUrls.Add(url);
                imagepublicid.Add(publicId);
            }


            product.ImageURL = imageUrls[0];
            await _productRepository.UpdateAsync(product);
            await _productRepository.AddImagesAsync(productId, imageUrls,imagepublicid);
            return true;
        }
        public async Task<bool> DeleteImageAsync(int productId, int imageId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new KeyNotFoundException($"Product {productId} not found.");

            int userId = GetCurrentUserId();
            var userRole = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (product.userid != userId&& userRole != "admin")
                throw new UnauthorizedAccessException("You cannot delete images from another seller's product.");
            var image=await _productRepository.getimagebyid(imageId,productId);
            if(image == null)
            {
                throw new KeyNotFoundException($"image with this id: {imageId} is not found");
            }
            await _cloudinaryService.DeleteImageAsync(image.imagepublicid);


            return await _productRepository.DeleteImageAsync(imageId, productId);
        }
        public async Task<bool> DeleteAllImagesAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            int userId = GetCurrentUserId();
            var userRole = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (product.userid != userId && userRole != "admin")
                throw new UnauthorizedAccessException("You cannot delete images from another seller's product.");
            var images=await _productRepository.GetProductImagesAsync(productId);
            if (images == null) { 
                throw new KeyNotFoundException($"no Images Found for this product: {productId}");   
            }
            foreach(var image in images)
            {
                await _cloudinaryService.DeleteImageAsync(image.imagepublicid);
            }

            return await _productRepository.DeleteAllImagesAsync(productId);
        }

        public async Task<ProductResponseDTO?> CreateAsync([FromBody] CreateProductDTO dto)
        {
            // Get the current user's ID from the claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");

            var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId);
            if (!categoryExists)
                throw new Exception($"Invalid Category ID: {dto.CategoryId}. The category does not exist.");

            var userExists = await _productRepository.UserExistsAsync(userId);
            if (!userExists)
                throw new Exception($"Invalid User ID: {userId}. The user does not exist.");

            var product = new Product
            {
                CategoryID = dto.CategoryId,
                userid = userId,
                ProductName = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageURL = dto.ImageUrl ?? "/images/default-product.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var createdProduct = await _productRepository.CreateNewAsync(product);
            return MapToResponseDto(createdProduct);
        }
        public async Task<IEnumerable<ProductResponseDTO>> GetByUserIdAsync(int userId)
        {
            var products = await _productRepository.GetByUserIdAsync(userId);
            return products.Select(MapToResponseDto);
        }
        public async Task<bool> DeleteAsync(int productId)
        {
            return await _productRepository.DeleteAsync(productId);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToResponseDto);

            return products.Select(p =>
            {
                var dto = new ProductResponseDTO
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                  
                };

                //Check if product is in an active Flash Sale
                var activeFlashSale = p.FlashSaleProducts?
                    .FirstOrDefault(fsp =>
                        fsp.FlashSale != null &&
                        fsp.FlashSale.IsActive &&
                        fsp.FlashSale.StartDate <= DateTime.Now &&
                        fsp.FlashSale.EndDate >= DateTime.Now
                    );

                if (activeFlashSale != null)
                {
                    dto.IsInFlashSale = true;
                    dto.FlashSalePrice = activeFlashSale.DiscountedPrice;
                    dto.FlashSaleEndDate = activeFlashSale.FlashSale.EndDate;
                    dto.FlashSaleID = activeFlashSale.FlashSaleID;
                }

                return dto;
            });
        }
        

        public async Task<IEnumerable<ProductResponseDTO>> GetByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return products.Select(MapToResponseDto);
        }

        public async Task<ProductDetailsDTO?> GetById(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");

            return MapToDetailDto(product);
        }

        public async Task<ProductResponseDTO?> UpdateAsync(int productId, [FromBody] UpdateProductDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Cannot update. Product with ID {productId} not found.");

            // Optional: Verify that the current user is the owner or an admin
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                if (product.userid != currentUserId && userRole != "admin")
                    throw new UnauthorizedAccessException("You are not authorized to update this product.");
            }

            if (dto.CategoryId.HasValue && dto.CategoryId.Value > 0)
            {
                var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId.Value);
                if (!categoryExists)
                    throw new Exception($"Invalid Category ID: {dto.CategoryId.Value}. The category does not exist.");

                product.CategoryID = dto.CategoryId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                product.ProductName = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                product.Description = dto.Description;

            if (dto.Price.HasValue && dto.Price.Value > 0)
                product.Price = dto.Price.Value;

            if (dto.Stock.HasValue && dto.Stock.Value >= 0)
                product.Stock = dto.Stock.Value;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                product.ImageURL = dto.ImageUrl;

            var updatedProduct = await _productRepository.UpdateAsync(product);
            if (updatedProduct == null)
                throw new Exception("Product update failed. Please try again.");

            return MapToResponseDto(updatedProduct);
        }

        private ProductResponseDTO MapToResponseDto(Product product)
        {
            return new ProductResponseDTO
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
                UserId = product.userid,
                SellerName = product.user?.UserRole == "admin"
                ? "zenon"
                : (product.user?.UserFirstName + " " + product.user?.UserLastName ?? "Unknown"),

                SellerEmail = product.user?.UserRole == "admin"
                ? "zenon@gmail.com"
                : (product.user?.UserEmail ?? "Unknown"),

                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageURL = product.ImageURL,
                CreatedAt = product.CreatedAt,

                Images = product.Images?.Select(img => new ProductImageDTO
                {
                    ImageId = img.ImageId,
                    Url = img.ImageUrl,
                    PublicId = img.imagepublicid
                }).ToList() ?? new List<ProductImageDTO>()
            };
        }


        private ProductDetailsDTO MapToDetailDto(Product product)
        {
            var reviews = product.Reviews?.ToList() ?? new List<Review>();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return new ProductDetailsDTO
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
                UserId = product.userid,
                SellerName = product.user?.UserRole == "admin"
                ? "zenon"
                : (product.user?.UserFirstName + " " + product.user?.UserLastName ?? "Unknown"),

                SellerEmail = product.user?.UserRole == "admin"
                ? "zenon@gmail.com"
                : (product.user?.UserEmail ?? "Unknown"),
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageURL = product.ImageURL,
                CreatedAt = product.CreatedAt,
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = reviews.Count,

                Images = product.Images?.Select(img => new ProductImageDTO
                {
                    ImageId = img.ImageId,
                    Url = img.ImageUrl,
                    PublicId = img.imagepublicid
                }).ToList() ?? new List<ProductImageDTO>()
            };
        }
        public async Task<PagedResult<ProductResponseDTO>> GetProductsAsync(ProductFilterParameters parameters)
        {
            // Get paginated products from repository
            var pagedProducts = await _productRepository.GetProductsAsync(parameters);

            // Map Product entities to ProductResponseDTO
            var productDtos = pagedProducts.Data.Select(p => MapToResponseDto(p)).ToList();

            // Return new PagedResult with mapped DTOs
            return new PagedResult<ProductResponseDTO>(
                productDtos,
                pagedProducts.PageNumber,
                pagedProducts.PageSize,
                pagedProducts.TotalRecords
            );
        }
        public async Task<bool> AddProductToFlashSaleAsync(int productId, AddProductToFlashSaleDto dto)
        {
            // Validation: DiscountedPrice less than OriginalPrice
            if (dto.DiscountedPrice >= dto.OriginalPrice)
                return false;

            var flashSaleProduct = new FlashSaleProduct
            {
                ProductID = productId,
                FlashSaleID = dto.FlashSaleID,
                OriginalPrice = dto.OriginalPrice,
                DiscountedPrice = dto.DiscountedPrice,
                StockLimit = dto.StockLimit
            };

            return await _productRepository.AddProductToFlashSaleAsync(productId, flashSaleProduct);
        }
        public async Task<AddProductToFlashSaleDto> UpdateFlashSaleItemAsync(int id, AddProductToFlashSaleDto dto)
        {
            var flashSale = await _flashSaleRepository.GetByIdAsync(id);

            if (flashSale == null)
                throw new KeyNotFoundException($"Flash sale with ID {id} not found.");

            // Basic validation
            if (!string.IsNullOrWhiteSpace(dto.Title))
                flashSale.Title = dto.Title;

            if (dto.StartDate != default)
                flashSale.StartDate = dto.StartDate;

            if (dto.EndDate != default)
            {
                if (dto.EndDate <= flashSale.StartDate)
                    throw new Exception("EndDate must be after StartDate.");

                flashSale.EndDate = dto.EndDate;
            }

            flashSale.IsActive = dto.IsActive;
            flashSale.MaxUsers = dto.MaxUsers;

            var updated = await _flashSaleRepository.UpdateAsync(flashSale);

            return new AddProductToFlashSaleDto
            {

                FlashSaleID = updated.FlashSaleID,
                Title = updated.Title,
                StartDate = updated.StartDate,
                EndDate = updated.EndDate,
                IsActive = updated.IsActive,
                MaxUsers = updated.MaxUsers,
                CreatedAt = updated.CreatedAt,
                ProductCount = updated.FlashSaleProducts?.Count ?? 0
            };
        }
        public async Task<List<AddProductToFlashSaleDto>> GetProductsByFlashSaleIdAsync(int flashSaleId)
        {
            var flashSale = await _flashSaleRepository.GetByFlashSaleIdAsync(flashSaleId);

            if (flashSale == null)
                throw new KeyNotFoundException($"Flash sale with ID {flashSaleId} not found.");

            return flashSale.Select(fp => new AddProductToFlashSaleDto
            {
                StartDate=fp.FlashSale.StartDate,
                EndDate=fp.FlashSale.EndDate,
                IsActive = fp.FlashSale.IsActive,
                CreatedAt=fp.FlashSale.CreatedAt,
                ProductCount=fp.FlashSale.FlashSaleProducts.Count,
                ProductID = fp.ProductID,
                ProductName = fp.Product.ProductName,
                FlashSaleID = fp.FlashSaleID,
                OriginalPrice = fp.OriginalPrice,
                DiscountedPrice = fp.DiscountedPrice,
                StockLimit = fp.StockLimit,
            }).ToList();
        }

        public async Task<List<AddProductToFlashSaleDto>> GetAllFlashSaleProductsAsync()
        {
            var items = await _flashSaleRepository.GetAllFlashSaleProductAsync();

          
          
            return items.Select(fp => new AddProductToFlashSaleDto
            {
                StartDate = fp.FlashSale.StartDate,
                EndDate = fp.FlashSale.EndDate,
                IsActive = fp.FlashSale.IsActive,
                CreatedAt = fp.FlashSale.CreatedAt,
                ProductCount = fp.FlashSale.FlashSaleProducts.Count,
                ProductID = fp.ProductID,
                ProductName = fp.Product.ProductName,
                FlashSaleID = fp.FlashSaleID,
                OriginalPrice = fp.OriginalPrice,
                DiscountedPrice = fp.DiscountedPrice,
                StockLimit = fp.StockLimit,
            }).ToList();
        }

        public async Task<bool> RemoveProductFromFlashSaleAsync(int productId, int flashSaleId)
        {
            return await _productRepository.RemoveProductFromFlashSaleAsync(productId, flashSaleId);
        }


    }
}