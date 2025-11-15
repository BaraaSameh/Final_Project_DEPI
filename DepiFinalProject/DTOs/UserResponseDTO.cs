namespace DepiFinalProject.DTOs
{
    public class UserResponseDTO
    {
        public int UserID { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; } = "";
        public int AddressNumber { get; set; }
        public int CartsNumber { get; set; }
        public int OrdersNumber { get; set; }
        public int ReviewsNumber { get; set; }
        public int WishListNumber { get; set; }
        public string RefershToken { get; set; }
    }
}
