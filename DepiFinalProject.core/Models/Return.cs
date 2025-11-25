using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Core.Models
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

        //handel refunds
        public string? RefundId { get; set; }  // PayPal Refund ID
        public decimal? RefundAmount { get; set; }  // Amount refunded
        public DateTime? RefundedAt { get; set; }  // When refund was processed
        public string? RefundStatus { get; set; }
    }
}
