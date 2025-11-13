namespace DepiFinalProject.DTOs
{
    public class AddToCartRequestDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class UpdateQuantityDto
    {
        /// <example>3</example>
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
