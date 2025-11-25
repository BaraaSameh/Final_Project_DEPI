using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Core.Models
{
    public class Wishlist
    {
        [Key]
        public int WishlistID { get; set; }

        // علاقة مع User
        [Required]
        public int UserID { get; set; }

        // علاقة مع Product
        [Required]
        public int ProductID { get; set; }

        public DateTime? AddedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}
