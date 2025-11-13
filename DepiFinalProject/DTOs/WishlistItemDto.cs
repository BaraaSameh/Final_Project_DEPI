namespace DepiFinalProject.DTOs
{
    public class WishlistItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class WishlistResponseDto
    {
        public int UserId { get; set; }
        public List<WishlistItemDto> Items { get; set; } = new();
    }
}
