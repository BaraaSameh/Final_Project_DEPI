using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class WishlistItemDto
    {
        [Required(ErrorMessage ="the product id is required")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "the product name is required")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "the product price is required")]
        public decimal Price { get; set; }
    }

    public class WishlistResponseDto
    {
        public int UserId { get; set; }
        public List<WishlistItemDto> Items { get; set; } = new();
    }
}
