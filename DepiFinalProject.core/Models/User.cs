using System.ComponentModel.DataAnnotations;
using System.Net;

namespace DepiFinalProject.Core.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        public string? UserPassword { get; set; }  // Nullable for Google users

        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? UserPhone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserRole { get; set; } = "client"; // مثل "Admin" أو "Customer"

        // Navigation Properties
        public virtual ICollection<Return> Returns { get; set; } = new List<Return>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    }
}
