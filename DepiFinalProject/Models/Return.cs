using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class Return
    {
        [Key]
        public int ReturnID { get; set; }

        // FK → Users table
        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [ForeignKey(nameof(OrderItem))]
        public int OrderItemID { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public string Status { get; set; }  

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public bool IsCancelled { get; set; } = false;

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual OrderItem OrderItem { get; set; }
    }
}
