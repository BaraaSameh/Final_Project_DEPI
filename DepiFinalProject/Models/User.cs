using System.ComponentModel.DataAnnotations;
using System.Net;

namespace DepiFinalProject.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        public string UserPassword { get; set; }  // هاش الباسوورد في الواقع
        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserRole { get; set; } = "client"; // مثل "Admin" أو "Customer"

        // Navigation Properties
        public virtual ICollection<Return> Returns { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }= new List<Wishlist>();

        //add refresh tokens list "Seif"
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
