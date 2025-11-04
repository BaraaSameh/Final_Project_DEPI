using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class ProductDTO
    {
        public class CreateDTO
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

        public class UpdateDTO
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
        public class ResponseDTO //create , update ,Get
        {
            public int ProductID { get; set; }
            public int CategoryID { get; set; }
            public string CategoryName { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string ImageURL { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsInStock => Stock > 0;
        }
        public class DetailsDTO : ResponseDTO
        {
            public double AverageRating { get; set; }
            public int TotalReviews { get; set; }
        }

    }
}
