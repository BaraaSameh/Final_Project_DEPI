using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Core.DTOs
{
    public class AddToCartRequestDto
    {
        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "product Quantity is required")]
        public int Quantity { get; set; }
    }
    public class UpdateQuantityDto
    {
        [Required(ErrorMessage = "product Quantity is required")]
        public int Quantity { get; set; }
    }

    public class CartItemDto
        {
            public int ProductId { get; set; }      
            public string ProductName { get; set; } 
            public decimal Price { get; set; }      
            public int Quantity { get; set; }     
        }
    public class CartResponseDto
    {
        public int UserId { get; set; }                    
        public List<CartItemDto> Items { get; set; }       
        public int TotalQuantity { get; set; }            
        public decimal TotalPrice { get; set; }          

        public CartResponseDto()
        {
            Items = new List<CartItemDto>();
        }
    }
}
