using DepiFinalProject.Commmon.Pagination;
using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class ProductDTO
    {
        public class CreateProductDTO
        {
            [Required]
            public int CategoryId { get; set; }

            [Required]
            [MaxLength(200)]
            public string Name { get; set; }

            [Required]
            [StringLength(2000)]
            public string Description { get; set; }

            [Required]
            public decimal Price { get; set; }

            [Required]
            public int Stock { get; set; }

            public string? ImageUrl { get; set; }
        }

        public class UpdateProductDTO
        {
            public int? CategoryId { get; set; }

            [MaxLength(200)]
            public string? Name { get; set; }

            [StringLength(2000)]
            public string? Description { get; set; }

            public decimal? Price { get; set; }

            public int? Stock { get; set; }

            public string? ImageUrl { get; set; }
        }

    
        public class ProductImageDTO
        {
            public int ImageId { get; set; }
            public string Url { get; set; }
            public string PublicId { get; set; }
        }
    

    public class ProductResponseDTO
        {
            public int ProductID { get; set; }
            public int CategoryID { get; set; }
            public string CategoryName { get; set; }
            public int UserId { get; set; }
            public string SellerName { get; set; }
            public string SellerEmail { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string ImageURL { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsInStock => Stock > 0;

            public List<ProductImageDTO> Images { get; set; } = new();
        }

        public class ProductDetailsDTO
        {
            public int ProductID { get; set; }
            public int CategoryID { get; set; }
            public string CategoryName { get; set; }
            public int UserId { get; set; }
            public string SellerName { get; set; }
            public string SellerEmail { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string ImageURL { get; set; }
            public DateTime CreatedAt { get; set; }

            public double AverageRating { get; set; }
            public int TotalReviews { get; set; }
            public bool IsInStock => Stock > 0;

            public List<ProductImageDTO> Images { get; set; } = new();
        }
        /// <summary>
        /// Product filtering and pagination parameters
        /// Inherits PageNumber and PageSize from PaginationParameters
        /// </summary>
        public class ProductFilterParameters : PaginationParameters
        {
            /// <summary>
            /// Optional: Filter products by category ID
            /// </summary>
            public int? CategoryID { get; set; }
        }

    }
}